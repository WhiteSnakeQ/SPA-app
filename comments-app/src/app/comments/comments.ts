import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommentModel } from '../models/comment';
import { CommentsService } from '../services/comments';
import { CommentItemComponent } from './comment-item/comment-item';
import { CommentFormComponent } from './comments-form/comments-form'

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
		sortField: 'userName' | 'email' | 'createdAt' = 'createdAt';
		sortDesc: boolean = true;

		replyParentId: number | null = null;
		showForm: boolean = false;

		constructor
		(private commentsService: CommentsService, private cdr: ChangeDetectorRef)
		{
		}

		ngOnInit(): void
		{
			this.loadComments();
		}

		loadComments(): void
		{
			this.commentsService
				.getComments(this.page, this.sortField, this.sortDesc)
				.subscribe({
					next: data =>
					{
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
			
			this.comments.unshift(comment);
			if (this.comments.length > 25)
			{
				this.comments.pop();
				this.hasNextPage = true;
			}
		}

		setSort (field: 'userName' | 'email' | 'createdAt'): void
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