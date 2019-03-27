/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';

// import * as os from 'os';
// import * as net from 'net';


// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';

// import { workspace, ExtensionContext, window } from 'vscode';
// import { LanguageClient, LanguageClientOptions, ServerOptions, StreamInfo } from 'vscode-languageclient';

function startedInDebugMode() {
    let args = process.execArgv;
    if (args) {
        return args.some((arg) => /^--inspect-brk=?/.test(arg));
    }
    return false;
}

export function activate(context: vscode.ExtensionContext) {
    startedInDebugMode();
    // const debugPort: number = workspace.getConfiguration().get('cheezls.debug.languageServerPort');

    context.subscriptions.push(vscode.commands.registerCommand("cheez.test", () => {
        vscode.window.showInformationMessage("cheez command");
    }));

    // let serverOptions: ServerOptions = null;

    if (!startedInDebugMode())
    {
        // vscode.window.showInformationMessage("Cheez extension started in release mode");

    //     // The server is implemented in C#
    //     let serverCommand: string = workspace.getConfiguration().get('cheezls.languageServerPath');
    //     if (serverCommand === null || serverCommand === undefined) {
    //         window.showErrorMessage("Cheez language server location was not specified. Please configure the path to the language server executable under 'cheezls.languageServerPath', then restart Visual Studio Code");
    //         return;
    //     }

    //     let commandOptions = { stdio: 'pipe' };

    //     serverOptions = (os.platform() === 'win32') ? {
    //             run: { command: serverCommand, options: commandOptions },
    //             debug: { command: serverCommand, options: commandOptions }
    //         } : {
    //             run: { command: 'mono', args: [serverCommand], options: commandOptions },
    //             debug: { command: 'mono', args: [serverCommand], options: commandOptions }
    //         }
    }
    else
    {
        vscode.window.showInformationMessage("Cheez extension started in debug mode");

    //     serverOptions = () => {
    //         let socket = net.createConnection({
    //             port: debugPort,
    //             localAddress: "127.0.0.1"
    //         });
    //         let result: StreamInfo = {
    //             writer: socket,
    //             reader: socket
    //         };
    //         return Promise.resolve(result);
    //     };
    }

    // // Options to control the language client
    // let clientOptions: LanguageClientOptions = {
    //     // Register the server for plain text documents
    //     documentSelector: [
    //         {
    //             scheme: 'file',
    //             language: 'cheezlang'
    //         }
    //     ],
    //     //documentSelector: [{scheme: 'file', language: 'che'}],
    //     synchronize: {
    //         // Synchronize the setting section 'languageServerExample' to the server
    //         configurationSection: 'cheezls',
    //         // Notify the server about file changes to '.clientrc files contain in the workspace
    //         fileEvents: workspace.createFileSystemWatcher('**/.clientrc')
    //     }
    // }

    // // Create the language client and start the client.
    // let lclient = new LanguageClient('cheezls', 'CheezLang Language Server', serverOptions, clientOptions);
    // let disposable = lclient.start();

    // // Push the disposable to the context's subscriptions so that the
    // // client can be deactivated on extension deactivation
    // context.subscriptions.push(disposable);
}
