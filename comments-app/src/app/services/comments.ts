import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommentsResponseModel } from '../models/comments-response';
import { CreateCommentModel } from '../models/create-comment';
import { CommentModel } from '../models/comment';

@Injectable({
    providedIn: 'root'
})

export class CommentsService
{
    private apiUrl = '/api/comments';

    constructor(private http: HttpClient)
    {
    }

    getComments(page: number, sort: string, desc: boolean) : Observable<CommentsResponseModel>
    {
        const params = new HttpParams().set('page', page).set('sort', sort).set('desc', desc);

        return this.http.get<CommentsResponseModel>
        (
            this.apiUrl,
            {
                params
            }
        );
    }

	createComment(data: CreateCommentModel)
	{
		const formData = new FormData();

		formData.append('userName',	data.userName);
		formData.append('email', data.email);
		formData.append('homepage',	data.homepage ?? '');
		formData.append('text',	data.text);
		formData.append('captchaId', data.captchaId);
		formData.append('captchaAnswer', data.captchaAnswer);

		if (data.parentId !== null)
			formData.append('parentId',	data.parentId.toString());

		for (const file of data.files)
			formData.append('files', file);

		for (const pair of formData.entries())
		{
			console.log(pair[0], pair[1]);
		}

		return this.http.post<CommentModel>(this.apiUrl, formData);
	}

	getCaptcha()
	{
		return this.http.get(`${this.apiUrl}/captcha`,
		{
			observe: 'response',
			responseType: 'blob'
		});
	}
}