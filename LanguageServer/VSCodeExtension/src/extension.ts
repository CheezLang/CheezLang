/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';

//import * as os from 'os';
import * as net from 'net';

import { workspace, ExtensionContext } from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, StreamInfo } from 'vscode-languageclient';
//import { TransportKind } from 'vscode-languageclient/lib/client';

export function activate(context: ExtensionContext) {

	// The server is implemented in C#
	//let serverCommand = 'D:\\Utilities\\CheezLanguageServer\\CheezLanguageServer.exe';
	//let commandOptions = { stdio: 'pipe' };

	// If the extension is launched in debug mode then the debug server options are used
	// Otherwise the run options are used
	// let serverOptions: ServerOptions =
	// 	(os.platform() === 'win32') ? {
	// 		run: { command: serverCommand, options: commandOptions },
	// 		debug: { command: serverCommand, options: commandOptions }
	// 	} : {
	// 			run: { command: 'mono', args: [serverCommand], options: commandOptions },
	// 			debug: { command: 'mono', args: [serverCommand], options: commandOptions }
	// 		}

	let serverOptions: ServerOptions = () => {
		let socket = net.createConnection({
			port: 5007,
			localAddress: "127.0.0.1"
		});
		let result: StreamInfo = {
			writer: socket,
			reader: socket
		};
		return Promise.resolve(result);
	};

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
			configurationSection: 'cheezls',
			// Notify the server about file changes to '.clientrc files contain in the workspace
			fileEvents: workspace.createFileSystemWatcher('**/.clientrc')
		}
	}

	// Create the language client and start the client.
	let disposable = new LanguageClient('cheezls', 'CheezLang Language Server', serverOptions, clientOptions).start();

	// Push the disposable to the context's subscriptions so that the 
	// client can be deactivated on extension deactivation
	context.subscriptions.push(disposable);
}
