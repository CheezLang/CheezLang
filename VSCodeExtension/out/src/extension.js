/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const path = require("path");
const os = require("os");
const vscode_1 = require("vscode");
const vscode_languageclient_1 = require("vscode-languageclient");
function activate(context) {
    // The server is implemented in C#
    let serverCommand = context.asAbsolutePath(path.join('..', 'LanguageServer', 'bin', 'Release', 'CheezLanguageServer.exe'));
    let serverCommandDebug = context.asAbsolutePath(path.join('..', 'LanguageServer', 'bin', 'Debug', 'CheezLanguageServer.exe'));
    let commandOptions = { stdio: 'pipe' };
    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    let serverOptions = (os.platform() === 'win32') ? {
        run: { command: serverCommand, options: commandOptions },
        debug: { command: serverCommandDebug, options: commandOptions }
    } : {
        run: { command: 'mono', args: [serverCommand], options: commandOptions },
        debug: { command: 'mono', args: [serverCommandDebug], options: commandOptions }
    };
    // Options to control the language client
    let clientOptions = {
        // Register the server for plain text documents
        documentSelector: [
            {
                scheme: 'file',
                language: 'cheezlang'
            }
        ],
        //documentSelector: [{scheme: 'file', language: 'che'}],
        synchronize: {
            // Synchronize the setting section 'languageServerExample' to the server
            configurationSection: 'cheezls',
            // Notify the server about file changes to '.clientrc files contain in the workspace
            fileEvents: vscode_1.workspace.createFileSystemWatcher('**/.clientrc')
        }
    };
    // Create the language client and start the client.
    let disposable = new vscode_languageclient_1.LanguageClient('cheezls', 'CheezLang Language Server', serverOptions, clientOptions).start();
    // Push the disposable to the context's subscriptions so that the 
    // client can be deactivated on extension deactivation
    context.subscriptions.push(disposable);
}
exports.activate = activate;
//# sourceMappingURL=extension.js.map