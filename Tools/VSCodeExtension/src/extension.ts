/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';

import * as net from 'net';
import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, StreamInfo } from 'vscode-languageclient';


export function activate(context: vscode.ExtensionContext) {
    const serverCommand: string = vscode.workspace.getConfiguration().get('cheez.languageServerPath');
    const debugPort: number = vscode.workspace.getConfiguration().get('cheez.languageServerPort');
    const useStdio: boolean = vscode.workspace.getConfiguration().get('cheez.use_std_io');

    // vscode.window.showInformationMessage("Cheez extension: " + debugPort + ", " + useStdio);

    let serverOptions: ServerOptions = null;

    if (useStdio)
    {
        // The server is implemented in C#
        if (serverCommand === null || serverCommand === undefined) {
            vscode.window.showErrorMessage("Cheez language server location was not specified. Please configure the path to the language server executable under 'cheez.languageServerPath', then restart Visual Studio Code");
            return;
        }

        serverOptions = {
            run: { command: serverCommand, args: ["--language-server-console", "dummy.che"] },
            debug: { command: serverCommand, args: ["--language-server-console", "dummy.che"] }
        };
    }
    else
    {
        serverOptions = () => {
            let socket = net.createConnection({
                port: debugPort,
                localAddress: "127.0.0.1"
            });
            let result: StreamInfo = {
                writer: socket,
                reader: socket
            };
            return Promise.resolve(result);
        };
    }

    // Options to control the language client
    let clientOptions: LanguageClientOptions = {
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
            configurationSection: 'cheez',
            // Notify the server about file changes to '.clientrc files contain in the workspace
            fileEvents: vscode.workspace.createFileSystemWatcher('**/.clientrc')
        }
    }

    // Create the language client and start the client.
    let lclient = new LanguageClient('cheez', 'Cheez Language Server', serverOptions, clientOptions);
    let disposable = lclient.start();

    // Push the disposable to the context's subscriptions so that the
    // client can be deactivated on extension deactivation
    context.subscriptions.push(disposable);
}
