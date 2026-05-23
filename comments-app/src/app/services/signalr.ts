import { Injectable } from '@angular/core';
import { CommentModel, FileModel } from '../models/comment';
import * as signalR from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class SignalrService 
{
	private hubConnection!: signalR.HubConnection;

	async startConnection(onComment: (comment: CommentModel) => void, onReply: (comment: CommentModel) => void, onFileReady: (comment: FileModel) => void): Promise<void>
	{
		if (this.hubConnection?.state === signalR.HubConnectionState.Connected)
            return;
		
		this.hubConnection = new signalR.HubConnectionBuilder()
			.withUrl('/commentsHub')
			.withAutomaticReconnect()
			.build();

		this.hubConnection.on('CommentCreated', onComment);

        this.hubConnection.on('ReplyCreated', onReply);

        this.hubConnection.on('FileReady', onFileReady)

		this.hubConnection.onreconnecting(error =>
        {
            console.log('SignalR reconnecting', error);
        });

        this.hubConnection.onreconnected(() =>
        {
            console.log('SignalR reconnected');
        });

        this.hubConnection.onclose(error =>
        {
            console.log('SignalR closed', error);
        });

		await this.hubConnection.start();

		console.log('SignalR Connected');
	}
}