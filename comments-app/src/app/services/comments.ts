import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommentsResponseModel } from '../models/comments-response';
import { CreateCommentModel } from '../models/create-comment';
import { CommentModel } from '../models/comment';
import { GET_COMMENTS, GET_REPLY } from '../graphql/commentQl';
import { CommentSearchModel } from '../models/commentSearch';

@Injectable({
    providedIn: 'root'
})

export class CommentsService
{
    private apiUrl = '/api/comments';
	private apiUrlSearch = '/api/comments/search'
	private apiUrlGQL = "/graphql"

    constructor(private http: HttpClient)
    {
    }

	getElasticSearch(compare: string) : Observable<any>
    {
		const params = new HttpParams().set('compare', compare);

        return this.http.get<CommentSearchModel>
        (
            this.apiUrlSearch,
            {
                params
            }
        );
    }

	getCommentsGraphQL(page: number, sort: string, desc: boolean) : Observable<any>
    {
        return this.http.post<any>
        (
            this.apiUrlGQL,
            {
                query: GET_COMMENTS,
				variables:
				{
					input:
					{
						page: page,
						sort: sort,
						desc: desc
					}
				}
            }
        );
    }

	getReplyCommentsGQL(CommentId: number) : Observable<any>
    {
        return this.http.post<any>
        (
            this.apiUrlGQL,
            {
                query: GET_REPLY,
				variables:
				{
					input:
					{
						commentId: CommentId
					}
				}
            }
        );
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
		formData.append('requestId', data.requestId);

		if (data.rootId && data.rootId.trim() !== '') 
			formData.append('rootId', data.rootId);

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