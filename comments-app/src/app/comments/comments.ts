import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommentModel } from '../models/comment';
import { CommentsService } from '../services/comments';
import { SignalrService } from '../services/signalr';
import { CommentItemComponent } from './comment-item/comment-item';
import { CommentFormComponent } from './comments-form/comments-form'
import { CommentSorting } from '../enums/sorting';

@Component({
    selector: 'app-comments',
    standalone: true,
    imports:
    [
        CommonModule,
        CommentItemComponent,
		CommentFormComponent
    ],
    templateUrl: './comments.html',
    styleUrls: ['./comments.css']
})

export class CommentsComponent
    implements OnInit
	{
		comments: CommentModel[] = [];
		hasNextPage: boolean = false;

		page: number = 0;
		sortField: CommentSorting = CommentSorting.CREATED_AT;
		sortDesc: boolean = true;

		replyParentId: number | null = null;
		showForm: boolean = false;

		isSelectFile: boolean = false;

		constructor (private commentsService: CommentsService, private cdr: ChangeDetectorRef, private signalr: SignalrService)
		{
		}

		async ngOnInit(): Promise<void>
		{
			await this.signalr.startConnection(
				comment => this.onCommentCreatedSignal(comment),
				reply => this.onReplyCreatedSignal(reply)
			);

			document.addEventListener
			(
				'visibilitychange',
				this.handleVisibilityChange
			);
			this.loadComments();
		}

		onFileSelecting(isSelected: boolean) : void
		{
			this.isSelectFile = isSelected;
		}

		handleVisibilityChange = (): void =>
		{
			if (document.visibilityState === 'visible')
			{
				this.loadComments();
			}
		}
		
		loadComments(): void
		{
			this.commentsService
				.getCommentsGraphQL(this.page, this.sortField, this.sortDesc)
				.subscribe({
					next: response =>
					{
						const data = response.data.comments;

						this.comments = data.items;
						this.hasNextPage = data.hasNextPage;
						this.cdr.detectChanges();
					},

					error: err =>
					{
						console.error(err);
					}
				});
		}

		openReply(parentId: number | null): void
		{
			this.replyParentId = parentId;

			this.showForm = true;
		}

		closeForm(): void
		{
			this.showForm = false;

			this.replyParentId = null;
		}

		onCommentCreated(comment: CommentModel): void 
		{
			this.closeForm();
		}

		onCommentCreatedSignal(comment: CommentModel): void 
		{	
			if (!this.shouldInsert)
			{
				this.loadComments()
				return;
			}
			
			this.insertComment(comment)
		}

		private shouldInsert(): boolean 
		{
			return (
				this.page === 0 &&
				this.sortField === CommentSorting.CREATED_AT &&
				this.sortDesc
			);
		}

		private insertComment(comment: CommentModel): void 
		{
			this.comments.unshift(comment);

			if (this.comments.length > 25) {

				this.comments.pop();
				this.hasNextPage = true;
			}
			this.cdr.detectChanges();
		}

		onReplyCreatedSignal(reply: CommentModel): void
		{
			
			const parent = this.findComment(reply.parentId, this.comments);
    		if (!parent)
        		return;

			parent.replyCount += 1; 
			parent.children?.unshift(reply);
			this.cdr.detectChanges();
		}

		findComment(parentId: number, comments?: CommentModel[]) : CommentModel | null
		{
			if (!comments)
				return null;
			for (const comment of comments)
			{
				if (comment.id == parentId)
					return comment;

				const children = this.findComment(parentId, comment.children);
				if (children)
					return children;
			}
			return null;
		}

		setSort (field: CommentSorting): void
		{
			this.page = 0
			if (this.sortField === field)
			{
				this.sortDesc = !this.sortDesc;
			}
			else
			{
				this.sortField = field;
				this.sortDesc = true;
			}

			this.loadComments();
		}

		
		nextPage(): void
		{
			if (this.hasNextPage != true)
				return

			this.page++;
			this.loadComments();
		}

		prevPage(): void
		{
			if (this.page <= 0)
				return

			this.page--;
			this.loadComments();
		}
	}
	