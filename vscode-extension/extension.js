'use strict';
const vscode = require('vscode');
const { execFile } = require('child_process');
const path = require('path');
const { KEYWORDS, FUNCTIONS, CONSTANTS, CLASSES, VALUE_MEMBERS } = require('./alx-data');

/**
 * ALX Language Support
 *  - Syntax colours (tmLanguage grammar), snippets and the ALX Night theme
 *  - Run file: play button, Ctrl+Alt+R, output panel
 *  - Completions for keywords, built-in functions, Vec2 and Entity
 *  - Hover help and signature help for built-ins
 *  - Outline of classes and functions
 *  - Your own functions and classes complete as you type
 */

const DOCS_URL = 'https://real-devalex.github.io/alx/';
let output = null;
let statusItem = null;

function md(text) {
    const m = new vscode.MarkdownString(text);
    m.isTrusted = false;
    return m;
}

/**
 * Where the alx program might be, best guess first.
 * On Windows the installer makes alx.bat, and VS Code cannot start a .bat file directly,
 * so we also try the real program, ALX.CLI.exe, on the PATH and in the install folder.
 */
function alxCandidates() {
    const configured = vscode.workspace.getConfiguration('alx').get('executablePath', 'alx');
    const list = [configured];
    if (process.platform === 'win32' && configured === 'alx') {
        list.push('ALX.CLI');
        const local = process.env.LOCALAPPDATA;
        if (local) list.push(path.join(local, 'ALX', 'ALX.CLI.exe'));
    }
    return list;
}

/** Runs alx with the given arguments, trying each place it could be until one starts. */
function runAlx(args, options, done) {
    const candidates = alxCandidates();
    const attempt = i => {
        execFile(candidates[i], args, options, (error, stdout, stderr) => {
            if (error && error.code === 'ENOENT' && i + 1 < candidates.length) { attempt(i + 1); return; }
            done(error, stdout, stderr);
        });
    };
    attempt(0);
}

/** Names the user declared in this file: functions, classes, and variables assigned at any depth. */
function declaredNames(document) {
    const text = document.getText();
    const out = { functions: new Set(), classes: new Set(), variables: new Set() };
    for (const m of text.matchAll(/\b(?:function|afun)\s+([A-Za-z_]\w*)/g)) out.functions.add(m[1]);
    for (const m of text.matchAll(/\bclass\s+([A-Za-z_]\w*)/g)) out.classes.add(m[1]);
    for (const m of text.matchAll(/^\s*(?:const\s+)?([A-Za-z_]\w*)\s*=(?!=)/gm)) out.variables.add(m[1]);
    out.functions.delete('constructor');
    return out;
}

function memberItems(table, kind) {
    const items = [];
    for (const [name, [type, doc]] of Object.entries(table)) {
        const item = new vscode.CompletionItem(name, type === 'method' ? vscode.CompletionItemKind.Method : vscode.CompletionItemKind.Property);
        item.detail = kind;
        item.documentation = md(doc);
        if (type === 'method') item.insertText = new vscode.SnippetString(`${name}($0)`);
        items.push(item);
    }
    return items;
}

function provideCompletions(document, position) {
    const line = document.lineAt(position).text.slice(0, position.character);
    const items = [];

    // after a dot: offer the members of every built-in kind (we do not track types)
    if (/\.\s*[A-Za-z_]*$/.test(line) && !/\.\.\s*\w*$/.test(line)) {
        const seen = new Set();
        const add = list => list.forEach(i => { if (!seen.has(i.label)) { seen.add(i.label); items.push(i); } });
        for (const [cls, info] of Object.entries(CLASSES)) add(memberItems(info.members, cls));
        add(memberItems(VALUE_MEMBERS.array, 'array'));
        add(memberItems(VALUE_MEMBERS.map, 'map'));
        add(memberItems(VALUE_MEMBERS.string, 'string'));
        return items;
    }

    for (const [name, doc, snippet] of KEYWORDS) {
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Keyword);
        item.documentation = md(doc);
        if (snippet) item.insertText = new vscode.SnippetString(snippet);
        items.push(item);
    }
    for (const [name, [sig, doc]] of Object.entries(FUNCTIONS)) {
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Function);
        item.detail = sig;
        item.documentation = md(doc);
        item.insertText = new vscode.SnippetString(`${name}($0)`);
        items.push(item);
    }
    for (const [name, [, doc]] of Object.entries(CONSTANTS)) {
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Constant);
        item.documentation = md(doc);
        items.push(item);
    }
    for (const [name, info] of Object.entries(CLASSES)) {
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Class);
        item.detail = info.ctor;
        item.documentation = md(info.doc);
        items.push(item);
    }
    const mine = declaredNames(document);
    for (const n of mine.functions) { const i = new vscode.CompletionItem(n, vscode.CompletionItemKind.Function); i.detail = 'your function'; i.insertText = new vscode.SnippetString(`${n}($0)`); items.push(i); }
    for (const n of mine.classes) { const i = new vscode.CompletionItem(n, vscode.CompletionItemKind.Class); i.detail = 'your class'; items.push(i); }
    for (const n of mine.variables) { const i = new vscode.CompletionItem(n, vscode.CompletionItemKind.Variable); i.detail = 'your variable'; items.push(i); }
    return items;
}

function provideHover(document, position) {
    const range = document.getWordRangeAtPosition(position, /[A-Za-z_]\w*/);
    if (!range) return null;
    const word = document.getText(range);

    if (FUNCTIONS[word]) {
        const [sig, doc] = FUNCTIONS[word];
        return new vscode.Hover(md(`\`\`\`alx\n${sig}\n\`\`\`\n${doc}`), range);
    }
    if (CONSTANTS[word]) return new vscode.Hover(md(`**${word}**\n\n${CONSTANTS[word][1]}`), range);
    if (CLASSES[word]) return new vscode.Hover(md(`\`\`\`alx\n${CLASSES[word].ctor}\n\`\`\`\n${CLASSES[word].doc}`), range);
    const kw = KEYWORDS.find(k => k[0] === word);
    if (kw) return new vscode.Hover(md(`**${word}**\n\n${kw[1]}`), range);

    // a member after a dot
    if (range.start.character > 0) {
        const before = document.getText(new vscode.Range(range.start.translate(0, -1), range.start));
        if (before === '.') {
            const tables = [...Object.entries(CLASSES).map(([k, v]) => [k, v.members]), ['array', VALUE_MEMBERS.array], ['map', VALUE_MEMBERS.map], ['string', VALUE_MEMBERS.string]];
            for (const [kind, table] of tables) {
                if (table[word]) return new vscode.Hover(md(`**${kind}.${word}**\n\n${table[word][1]}`), range);
            }
        }
    }
    return null;
}

function provideSignature(document, position) {
    // find the function name before the nearest unclosed "("
    const text = document.lineAt(position).text.slice(0, position.character);
    let depth = 0, comma = 0, i = text.length - 1;
    for (; i >= 0; i--) {
        const c = text[i];
        if (c === ')') depth++;
        else if (c === '(') { if (depth === 0) break; depth--; }
        else if (c === ',' && depth === 0) comma++;
    }
    if (i < 0) return null;
    const m = /([A-Za-z_]\w*)\s*$/.exec(text.slice(0, i));
    if (!m || !FUNCTIONS[m[1]]) return null;
    const [sig, doc, args] = FUNCTIONS[m[1]];
    const info = new vscode.SignatureInformation(sig, md(doc));
    const names = (/\((.*)\)/.exec(sig) || [null, ''])[1].split(',').map(s => s.trim()).filter(Boolean);
    info.parameters = names.map((n, idx) => new vscode.ParameterInformation(n, md(args[idx] || '')));
    const help = new vscode.SignatureHelp();
    help.signatures = [info];
    help.activeSignature = 0;
    help.activeParameter = Math.min(comma, Math.max(names.length - 1, 0));
    return help;
}

function provideSymbols(document) {
    const symbols = [];
    const lines = document.getText().split(/\r?\n/);
    lines.forEach((text, n) => {
        let m = /^\s*class\s+([A-Za-z_]\w*)/.exec(text);
        if (m) {
            const range = new vscode.Range(n, 0, n, text.length);
            symbols.push(new vscode.DocumentSymbol(m[1], 'class', vscode.SymbolKind.Class, range, range));
            return;
        }
        m = /^\s*(?:function|afun)\s+([A-Za-z_]\w*)/.exec(text);
        if (m) {
            const range = new vscode.Range(n, 0, n, text.length);
            const method = /^\s+/.test(text);
            symbols.push(new vscode.DocumentSymbol(m[1], method ? 'method' : 'function', method ? vscode.SymbolKind.Method : vscode.SymbolKind.Function, range, range));
        }
    });
    return symbols;
}

async function runFile() {
    const editor = vscode.window.activeTextEditor;
    if (!editor) { vscode.window.showErrorMessage('No active editor. Open an .alx file first.'); return; }
    const document = editor.document;
    if (document.languageId !== 'alx') { vscode.window.showErrorMessage('This command only works with ALX files (.alx).'); return; }
    if (document.isDirty) await document.save();

    const filePath = document.fileName;
    output.clear();
    output.show(true);
    output.appendLine(`> alx run "${filePath}"`);
    output.appendLine('');

    const started = Date.now();
    runAlx(['run', filePath], { cwd: path.dirname(filePath), timeout: 30000 }, (error, stdout, stderr) => {
        if (stdout) output.appendLine(stdout);
        if (stderr) output.appendLine(stderr);
        if (error && error.code === 'ENOENT') {
            output.appendLine('I could not find the alx program.');
            output.appendLine('If you just installed ALX, close ALL VS Code windows and open VS Code again so it can see the new PATH.');
            output.appendLine('Otherwise set "alx.executablePath" in your settings.');
            output.appendLine(`Install guide: ${DOCS_URL}learn/install/`);
        } else if (error && error.killed) {
            output.appendLine('The program ran for more than 30 seconds, so it was stopped.');
        }
        output.appendLine(`[Finished in ${Date.now() - started}ms]`);
        const failed = error && !error.killed;
        vscode.window.setStatusBarMessage(failed ? '$(error) ALX: the program stopped with an error' : '$(check) ALX: done', 3000);
    });
}

function showVersion() {
    runAlx(['version'], { timeout: 8000 }, (error, stdout) => {
        if (error) { vscode.window.showWarningMessage('ALX is not installed or not on your PATH. Set "alx.executablePath" in settings.'); return; }
        vscode.window.showInformationMessage(String(stdout).trim().split(/\r?\n/)[0] || 'ALX');
    });
}

function updateStatus(editor) {
    if (!statusItem) return;
    const on = vscode.workspace.getConfiguration('alx').get('showStatusBar', true) && editor && editor.document.languageId === 'alx';
    if (on) statusItem.show(); else statusItem.hide();
}

function activate(context) {
    output = vscode.window.createOutputChannel('ALX Output');
    const sel = { language: 'alx' };

    statusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 100);
    statusItem.text = '$(play) ALX';
    statusItem.tooltip = 'Run this ALX file (Ctrl+Alt+R)';
    statusItem.command = 'alx.runFile';
    updateStatus(vscode.window.activeTextEditor);

    context.subscriptions.push(
        output,
        statusItem,
        vscode.commands.registerCommand('alx.runFile', runFile),
        vscode.commands.registerCommand('alx.showVersion', showVersion),
        vscode.commands.registerCommand('alx.openDocs', () => vscode.env.openExternal(vscode.Uri.parse(DOCS_URL))),
        vscode.window.onDidChangeActiveTextEditor(updateStatus),
        vscode.workspace.onDidChangeConfiguration(() => updateStatus(vscode.window.activeTextEditor)),
        vscode.languages.registerCompletionItemProvider(sel, { provideCompletionItems: provideCompletions }, '.'),
        vscode.languages.registerHoverProvider(sel, { provideHover }),
        vscode.languages.registerSignatureHelpProvider(sel, { provideSignatureHelp: provideSignature }, '(', ','),
        vscode.languages.registerDocumentSymbolProvider(sel, { provideDocumentSymbols: provideSymbols })
    );
}

function deactivate() {}

module.exports = { activate, deactivate };
