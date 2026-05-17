export interface FileModel
{
    id: number;

    fileUrl: string;

    fileName: string;

    fileType: string;
}

export interface CommentModel
{
    id: number;

    userName: string;

    email: string;

    text: string;

    createdAt: Date;

    files: FileModel[];

    children: CommentModel[];
}