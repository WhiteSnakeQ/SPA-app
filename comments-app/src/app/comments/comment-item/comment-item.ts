import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommentModel } from '../../models/comment';
import { CommonModule } from '@angular/common';
import { CommentFormComponent } from '../comments-form/comments-form';
import { FileModel } from '../../models/comment';

@Component({
	selector: 'app-comment-item',
	standalone: true,
	imports: [
		CommonModule, 
		CommentFormComponent
	],
	templateUrl: './comment-item.html',
	styleUrls: ['./comment-item.css']
	})

export class CommentItemComponent {
	@Input() comment!: CommentModel;
	@Output() sort = new EventEmitter<'userName' | 'email' | 'createdAt'>();

	showReplyForm: boolean = false;

	openReply(): void
	{
		this.showReplyForm = !this.showReplyForm;
	}

	createdReply(reply: CommentModel): void
	{
		this.comment.children.push(reply);

    	this.showReplyForm = false;
	}

	isImage(file: FileModel): boolean
	{
		return (
			file.fileType.startsWith('PNG') ||
			file.fileType.startsWith('JPG') ||
			file.fileType.startsWith('GIF') ||
			file.fileType.startsWith('Image'))
	}
}