import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommentModel } from '../../models/comment';
import { CommonModule } from '@angular/common';
import { CommentFormComponent } from '../comments-form/comments-form';
import { FileModel } from '../../models/comment';
import { CommentSorting } from '../../enums/sorting';
import { CommentsService } from '../../services/comments';

@Component
({
	selector: 'app-comment-item',
	standalone: true,
	imports: [
		CommonModule, 
		CommentFormComponent
	],
	templateUrl: './comment-item.html',
	styleUrls: ['./comment-item.css']
})

export class CommentItemComponent 
{
	@Input() comment!: CommentModel;
	@Output() sort = new EventEmitter<CommentSorting>();
	@Output() fileSelecting = new EventEmitter<boolean>();

	showReplyForm: boolean = false;
	CommentSorting = CommentSorting;

	showChildren: boolean = false;

	hasLoadedChildren: boolean = false;
	constructor (private commentsService: CommentsService, private cdr: ChangeDetectorRef )
	{

	}

	openReply(): void
	{
		this.showReplyForm = !this.showReplyForm;
	}

	closeForm(reply: CommentModel): void
	{
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

	showChildrens()
	{
		if (!this.hasLoadedChildren )
		{
			this.loadReply();
			this.hasLoadedChildren  = true;
		}
			
		this.showChildren = !this.showChildren
	}

	loadReply(): void
	{
		this.commentsService
			.getReplyCommentsGQL(this.comment.id)
			.subscribe({
				next: response =>
				{
					const data = response.data;
					this.comment.children = data.replyComments;
					this.cdr.detectChanges();
				},

				error: err =>
				{
					console.error(err);
				}
			});
	}
}