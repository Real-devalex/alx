'use strict';
// Runs the editor features against a tiny fake of the VS Code API, so they can be tested without opening VS Code.
const assert = require('assert');
const Module = require('module');

class MarkdownString { constructor(v) { this.value = v; } }
class CompletionItem { constructor(label, kind) { this.label = label; this.kind = kind; } }
class SnippetString { constructor(v) { this.value = v; } }
class Hover { constructor(contents, range) { this.contents = contents; this.range = range; } }
class Position { constructor(l, c) { this.line = l; this.character = c; } translate(dl, dc) { return new Position(this.line + dl, this.character + dc); } }
class Range { constructor(a, b, c, d) { if (typeof a === 'number') { this.start = new Position(a, b); this.end = new Position(c, d); } else { this.start = a; this.end = b; } } }
class SignatureInformation { constructor(label, doc) { this.label = label; this.documentation = doc; } }
class ParameterInformation { constructor(label, doc) { this.label = label; this.documentation = doc; } }
class SignatureHelp {}
class DocumentSymbol { constructor(name, detail, kind) { this.name = name; this.detail = detail; this.kind = kind; } }

const registered = {};
const fake = {
    MarkdownString, CompletionItem, SnippetString, Hover, Range, Position, SignatureInformation, ParameterInformation, SignatureHelp, DocumentSymbol,
    CompletionItemKind: { Keyword: 1, Function: 2, Constant: 3, Class: 4, Variable: 5, Method: 6, Property: 7 },
    SymbolKind: { Class: 1, Function: 2, Method: 3 },
    StatusBarAlignment: { Right: 2 },
    window: {
        createOutputChannel: () => ({ dispose() {}, clear() {}, show() {}, appendLine() {} }),
        createStatusBarItem: () => ({ show() {}, hide() {}, dispose() {} }),
        onDidChangeActiveTextEditor: () => ({ dispose() {} }),
        activeTextEditor: undefined,
    },
    workspace: { getConfiguration: () => ({ get: (_k, d) => d }), onDidChangeConfiguration: () => ({ dispose() {} }) },
    commands: { registerCommand: () => ({ dispose() {} }) },
    env: { openExternal() {} },
    Uri: { parse: s => s },
    languages: {
        registerCompletionItemProvider: (_s, p) => { registered.completion = p; return { dispose() {} }; },
        registerHoverProvider: (_s, p) => { registered.hover = p; return { dispose() {} }; },
        registerSignatureHelpProvider: (_s, p) => { registered.signature = p; return { dispose() {} }; },
        registerDocumentSymbolProvider: (_s, p) => { registered.symbols = p; return { dispose() {} }; },
    },
};

const realLoad = Module._load;
Module._load = function (request, ...rest) { return request === 'vscode' ? fake : realLoad.call(this, request, ...rest); };
const ext = require('../extension');
ext.activate({ subscriptions: [] });

function doc(text) {
    const lines = text.split('\n');
    return {
        getText: range => (range ? textIn(lines, range) : text),
        lineAt: pos => ({ text: lines[typeof pos === 'number' ? pos : pos.line] }),
        getWordRangeAtPosition: (pos, re) => {
            const line = lines[pos.line];
            const rx = new RegExp(re.source, 'g');
            let m;
            while ((m = rx.exec(line))) { if (pos.character >= m.index && pos.character <= m.index + m[0].length) return new Range(pos.line, m.index, pos.line, m.index + m[0].length); }
            return undefined;
        },
    };
}
function textIn(lines, r) { return lines[r.start.line].slice(r.start.character, r.end.character); }

let passed = 0;
function test(name, fn) { fn(); passed++; console.log('  ok  ' + name); }

test('completion offers built-in functions and classes', () => {
    const items = registered.completion.provideCompletionItems(doc('x = '), new Position(0, 4));
    const names = items.map(i => i.label);
    for (const n of ['print', 'sqrt', 'lerp', 'randomInt', 'Vec2', 'Entity', 'PI', 'function', 'class']) assert.ok(names.includes(n), n + ' missing');
});

test('completion after a dot offers members', () => {
    const items = registered.completion.provideCompletionItems(doc('hero.'), new Position(0, 5));
    const names = items.map(i => i.label);
    for (const n of ['update', 'position', 'add', 'push', 'keys']) assert.ok(names.includes(n), n + ' missing');
    assert.ok(!names.includes('print'));
});

test('completion after a range does not offer members', () => {
    const items = registered.completion.provideCompletionItems(doc('for i in 1..'), new Position(0, 12));
    assert.ok(items.map(i => i.label).includes('print'));
});

test('completion includes the names declared in the file', () => {
    const items = registered.completion.provideCompletionItems(doc('function heal(a) {}\nclass Boss {}\nscore = 0\n'), new Position(2, 0));
    const names = items.map(i => i.label);
    for (const n of ['heal', 'Boss', 'score']) assert.ok(names.includes(n), n + ' missing');
    assert.ok(!names.includes('constructor'));
});

test('hover explains a built-in function', () => {
    const h = registered.hover.provideHover(doc('x = lerp(0, 10, 0.5)'), new Position(0, 6));
    assert.ok(h.contents.value.includes('lerp(a, b, t)'));
});

test('hover explains a keyword and a class', () => {
    assert.ok(registered.hover.provideHover(doc('class Boss {}'), new Position(0, 2)).contents.value.includes('blueprint'));
    assert.ok(registered.hover.provideHover(doc('v = new Vec2(1, 2)'), new Position(0, 11)).contents.value.includes('new Vec2(x, y)'));
});

test('hover explains a member after a dot', () => {
    const h = registered.hover.provideHover(doc('hero.update(1)'), new Position(0, 7));
    assert.ok(h.contents.value.includes('moves by velocity'));
});

test('hover is empty for unknown words', () => {
    assert.strictEqual(registered.hover.provideHover(doc('mystery = 1'), new Position(0, 2)), null);
});

test('signature help follows the argument you are on', () => {
    const first = registered.signature.provideSignatureHelp(doc('lerp(0, '), new Position(0, 8));
    assert.strictEqual(first.signatures[0].label, 'lerp(a, b, t)');
    assert.strictEqual(first.activeParameter, 1);
    const third = registered.signature.provideSignatureHelp(doc('lerp(0, 10, '), new Position(0, 12));
    assert.strictEqual(third.activeParameter, 2);
});

test('signature help ignores commas inside nested calls', () => {
    const h = registered.signature.provideSignatureHelp(doc('clamp(max(1, 2), '), new Position(0, 17));
    assert.strictEqual(h.signatures[0].label, 'clamp(x, low, high)');
    assert.strictEqual(h.activeParameter, 1);
});

test('outline lists classes, methods and functions', () => {
    const symbols = registered.symbols.provideDocumentSymbols(doc('class Player {\n    function hit(o) {}\n}\nfunction add(a, b) {}\n'));
    assert.deepStrictEqual(symbols.map(s => s.name + ':' + s.detail), ['Player:class', 'hit:method', 'add:function']);
});

console.log(`\n${passed} editor checks passed`);
