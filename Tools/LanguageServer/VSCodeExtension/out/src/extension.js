/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const os = require("os");
const net = require("net");
const vscode_1 = require("vscode");
const vscode_languageclient_1 = require("vscode-languageclient");
function startedInDebugMode() {
    let args = process.execArgv;
    if (args) {
        return args.some((arg) => /^--inspect-brk=?/.test(arg));
    }
    return false;
}
function activate(context) {
    const debugPort = vscode_1.workspace.getConfiguration().get('cheezls.debug.languageServerPort');
    let serverOptions = null;
    if (!startedInDebugMode()) {
        vscode_1.window.showInformationMessage("Cheez extension started in release mode");
        // The server is implemented in C#
        let serverCommand = vscode_1.workspace.getConfiguration().get('cheezls.languageServerPath');
        if (serverCommand === null || serverCommand === undefined) {
            vscode_1.window.showErrorMessage("Cheez language server location was not specified. Please configure the path to the language server executable under 'cheezls.languageServerPath', then restart Visual Studio Code");
            return;
        }
        let commandOptions = { stdio: 'pipe' };
        serverOptions = (os.platform() === 'win32') ? {
            run: { command: serverCommand, options: commandOptions },
            debug: { command: serverCommand, options: commandOptions }
        } : {
            run: { command: 'mono', args: [serverCommand], options: commandOptions },
            debug: { command: 'mono', args: [serverCommand], options: commandOptions }
        };
    }
    else {
        vscode_1.window.showInformationMessage("Cheez extension started in debug mode");
        serverOptions = () => {
            let socket = net.createConnection({
                port: debugPort,
                localAddress: "127.0.0.1"
            });
            let result = {
                writer: socket,
                reader: socket
            };
            return Promise.resolve(result);
        };
    }
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
    let lclient = new vscode_languageclient_1.LanguageClient('cheezls', 'CheezLang Language Server', serverOptions, clientOptions);
    let disposable = lclient.start();
    // Push the disposable to the context's subscriptions so that the 
    // client can be deactivated on extension deactivation
    context.subscriptions.push(disposable);
}
exports.activate = activate;
//# sourceMappingURL=extension.js.map