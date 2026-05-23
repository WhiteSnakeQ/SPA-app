import { UUID } from 'node:crypto';

export interface FileModel
{
    id: number;
    commentId: number;

    fileUrl: string;

    fileName: string;

    fileType: string;
}

export interface CommentModel
{
    id: number;

	parentId: number;

    rootId: UUID;

    userName: string;

    email: string;

    text: string;

    createdAt: Date;

    files?: FileModel[];

    children?: CommentModel[];

	replyCount: number;
}