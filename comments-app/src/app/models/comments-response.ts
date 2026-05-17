import { CommentModel } from './comment';

export interface CommentsResponseModel
{
    items: CommentModel[];
    hasNextPage: boolean;
}