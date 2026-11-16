'use strict';

/**
 * Everything the editor knows about ALX: keywords, built-in functions, classes and their members.
 * One place, so completions, hover help and signature help always agree.
 */

const KEYWORDS = [
    ['if', 'Run code only when a condition is true.', 'if ${1:condition} {\n\t$0\n}'],
    ['else', 'The other choice of an if.', 'else {\n\t$0\n}'],
    ['while', 'Repeat while a condition is true.', 'while ${1:condition} {\n\t$0\n}'],
    ['for', 'Repeat for every number in a range. The end is not included: 1..5 counts 1, 2, 3, 4.', 'for ${1:i} in ${2:1}..${3:10} {\n\t$0\n}'],
    ['in', 'Used in a for loop: for i in 1..5', null],
    ['function', 'Make a function you can call again and again.', 'function ${1:name}(${2:args}) {\n\t$0\n}'],
    ['afun', 'A shorter way to write function.', 'afun ${1:name}(${2:args}) {\n\t$0\n}'],
    ['lambda', 'A small function with no name: lambda(x) { x * 2 }', 'lambda(${1:x}) { ${2:x} }'],
    ['return', 'Give a value back from a function.', null],
    ['break', 'Leave the loop right now.', null],
    ['continue', 'Skip to the next round of the loop.', null],
    ['class', 'A blueprint for objects.', 'class ${1:Name} {\n\tfunction constructor(${2:args}) {\n\t\t$0\n\t}\n}'],
    ['extends', 'Build a class on top of another class.', null],
    ['new', 'Make an object from a class: new Player("Hero")', null],
    ['this', 'The object a method belongs to.', null],
    ['super', 'Call the parent class constructor or method.', null],
    ['import', 'Load another ALX file as a module.', 'import ${1:module}'],
    ['from', 'Used with import.', null],
    ['as', 'Give an import another name: import math as m', null],
    ['const', 'A value that can never change.', 'const ${1:NAME} = ${2:value}'],
    ['print', 'Show something on the screen.', 'print(${1:value})'],
    ['and', 'True when both sides are true.', null],
    ['or', 'True when either side is true.', null],
    ['not', 'Flips true to false and false to true.', null],
    ['true', 'The boolean true.', null],
    ['false', 'The boolean false.', null],
    ['null', 'Nothing.', null],
];

/** name: [signature, description, [argument docs]] */
const FUNCTIONS = {
    print: ['print(value)', 'Shows a value on the screen.', ['What to show']],
    input: ['input(prompt)', 'Asks the player to type something and returns it as text.', ['Text to show first']],
    type: ['type(value)', 'Returns the kind of a value: "integer", "float", "string", "boolean", "array", "map", "null"...', ['Any value']],
    sqrt: ['sqrt(x)', 'Square root of x.', ['A number']],
    pow: ['pow(x, y)', 'x to the power y.', ['The base', 'The power']],
    abs: ['abs(x)', 'The positive version of x.', ['A number']],
    sign: ['sign(x)', 'Returns -1, 0 or 1 depending on the sign of x.', ['A number']],
    floor: ['floor(x)', 'Rounds down to a whole number.', ['A number']],
    ceil: ['ceil(x)', 'Rounds up to a whole number.', ['A number']],
    round: ['round(x)', 'Rounds to the nearest whole number.', ['A number']],
    min: ['min(a, b)', 'The smaller of two numbers.', ['First number', 'Second number']],
    max: ['max(a, b)', 'The larger of two numbers.', ['First number', 'Second number']],
    clamp: ['clamp(x, low, high)', 'Keeps x between low and high.', ['The number', 'Smallest allowed', 'Largest allowed']],
    lerp: ['lerp(a, b, t)', 'A point between a and b. t = 0 gives a, t = 1 gives b.', ['Start', 'End', 'How far, 0 to 1']],
    sin: ['sin(x)', 'Sine of an angle in radians.', ['Angle in radians']],
    cos: ['cos(x)', 'Cosine of an angle in radians.', ['Angle in radians']],
    tan: ['tan(x)', 'Tangent of an angle in radians.', ['Angle in radians']],
    atan2: ['atan2(y, x)', 'The angle of the point (x, y) in radians.', ['y', 'x']],
    toRadians: ['toRadians(degrees)', 'Converts degrees to radians.', ['Degrees']],
    toDegrees: ['toDegrees(radians)', 'Converts radians to degrees.', ['Radians']],
    random: ['random()', 'A random float from 0 up to (not including) 1.', []],
    randomInt: ['randomInt(low, high)', 'A random whole number from low to high, both included.', ['Smallest', 'Largest']],
    time: ['time()', 'Seconds since 1970 as a float. Subtract two times to measure a gap.', []],
    sleep: ['sleep(ms)', 'Waits for ms milliseconds (up to 10000).', ['Milliseconds']],
};

const CONSTANTS = {
    PI: ['PI', 'The number pi, about 3.14159.'],
    TAU: ['TAU', 'Two times pi, a full turn in radians.'],
};

const CLASSES = {
    Vec2: {
        ctor: 'new Vec2(x, y)',
        doc: 'A point or direction with an x and a y.',
        members: {
            x: ['property', 'The horizontal number.'],
            y: ['property', 'The vertical number.'],
            add: ['method', 'add(other): a new Vec2, this plus other.'],
            sub: ['method', 'sub(other): a new Vec2, this minus other.'],
            scale: ['method', 'scale(k): a new Vec2 with both numbers times k.'],
            dot: ['method', 'dot(other): the dot product.'],
            length: ['method', 'length(): how long the vector is.'],
            distanceTo: ['method', 'distanceTo(other): distance between two points.'],
            normalized: ['method', 'normalized(): the same direction with length 1.'],
            lerp: ['method', 'lerp(other, t): a point between this and other.'],
            toString: ['method', 'toString(): text like (3, 4).'],
        },
    },
    Entity: {
        ctor: 'new Entity(name, x, y)',
        doc: 'A thing in your game with a position and a speed.',
        members: {
            name: ['property', 'The entity name.'],
            position: ['property', 'A Vec2 with where it is.'],
            velocity: ['property', 'A Vec2 with how fast it moves, per second.'],
            alive: ['property', 'true until you set it to false.'],
            update: ['method', 'update(dt): moves by velocity times dt.'],
            moveTo: ['method', 'moveTo(x, y): jumps to a position.'],
            distanceTo: ['method', 'distanceTo(other): distance to another entity.'],
        },
    },
};

/** Methods and properties of the built-in value types. */
const VALUE_MEMBERS = {
    array: {
        length: ['property', 'How many items.'],
        first: ['property', 'The first item.'],
        last: ['property', 'The last item.'],
        push: ['method', 'push(x): add to the end.'],
        pop: ['method', 'pop(): remove and return the last item.'],
        contains: ['method', 'contains(x): true if x is in the array.'],
        indexOf: ['method', 'indexOf(x): position of x, or -1.'],
        join: ['method', 'join(separator): all items as one text.'],
        reverse: ['method', 'reverse(): reverse the order.'],
    },
    map: {
        length: ['property', 'How many entries.'],
        keys: ['method', 'keys(): an array of the names.'],
        values: ['method', 'values(): an array of the values.'],
        containsKey: ['method', 'containsKey(key): true if the key exists.'],
        get: ['method', 'get(key, default): the value, or default if missing.'],
    },
    string: {
        length: ['property', 'How many characters.'],
    },
};

module.exports = { KEYWORDS, FUNCTIONS, CONSTANTS, CLASSES, VALUE_MEMBERS };
