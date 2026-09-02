const vscode = require('vscode');
const { execFile } = require('child_process');
const path = require('path');

/**
 * ALX Language Support Extension
 * Provides:
 * - Syntax highlighting (via tmLanguage grammar)
 * - Run ALX file (play button + Ctrl+Alt+R)
 * - Output panel for program output
 */

let outputChannel = null;

function activate(context) {
    console.log('ALX extension activated');

    // Create output channel for ALX output
    outputChannel = vscode.window.createOutputChannel('ALX Output');

    // Register the run command
    const runCommand = vscode.commands.registerCommand('alx.runFile', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showErrorMessage('No active editor. Open an .alx file first.');
            return;
        }

        const document = editor.document;
        if (document.languageId !== 'alx') {
            vscode.window.showErrorMessage('This command only works with ALX files (.alx).');
            return;
        }

        // Save the file first
        if (document.isDirty) {
            await document.save();
        }

        const filePath = document.fileName;
        const config = vscode.workspace.getConfiguration('alx');
        const alxPath = config.get('executablePath', 'alx');

        // Show output channel
        outputChannel.clear();
        outputChannel.show(true);
        outputChannel.appendLine(`> alx "${filePath}"`);
        outputChannel.appendLine('');

        // Run the ALX file
        const startTime = Date.now();
        const process = execFile(alxPath, [filePath], {
            cwd: path.dirname(filePath),
            timeout: 30000 // 30 second timeout
        }, (error, stdout, stderr) => {
            const elapsed = Date.now() - startTime;

            if (stdout) {
                outputChannel.appendLine(stdout);
            }

            if (stderr) {
                outputChannel.appendLine(stderr);
            }

            if (error && error.killed) {
                outputChannel.appendLine('');
                outputChannel.appendLine('⚠ Process timed out after 30 seconds.');
            }

            outputChannel.appendLine('');
            outputChannel.appendLine(`[Finished in ${elapsed}ms]`);

            if (error && error.code !== 0 && !error.killed) {
                // Show error in status bar briefly
                vscode.window.setStatusBarMessage('$(error) ALX: Execution failed', 3000);
            } else {
                vscode.window.setStatusBarMessage('$(check) ALX: Execution complete', 2000);
            }
        });
    });

    context.subscriptions.push(runCommand);
    context.subscriptions.push(outputChannel);
}

function deactivate() {
    if (outputChannel) {
        outputChannel.dispose();
    }
}

module.exports = { activate, deactivate };
