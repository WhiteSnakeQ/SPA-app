export interface CreateCommentModel
{
    userName: string;
    email: string;
    homepage?: string;
    text: string;
    parentId: number | null;

    files: File[];
	
    captchaId: string;
    captchaAnswer: string;
}