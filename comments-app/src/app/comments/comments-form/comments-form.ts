import { Component, EventEmitter, Input, Output, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommentsService } from '../../services/comments';
import { CreateCommentModel } from '../../models/create-comment';
import { CommentModel } from '../../models/comment';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { urlValidator } from '../../shared/validators/url.validator';

@Component({
    selector: 'app-comment-form',
    standalone: true,
    imports:
    [
        CommonModule,
        ReactiveFormsModule
    ],
    templateUrl: './comments-form.html',
    styleUrls: ['./comments-form.css']
})

export class CommentFormComponent
{
    @Input() parentId: number | null = null;
    @Output() created = new EventEmitter<CommentModel>();
    @Output() closed = new EventEmitter<void>();
	@Output() fileSelecting = new EventEmitter<boolean>();

	@ViewChild('textArea') textArea!: ElementRef<HTMLTextAreaElement>;

	captchaImage: string = '';
	form: FormGroup;

	files: File[] = [];
	fileErrors: string[] = [];
	captchaId: string = '';

	constructor(private commentsService: CommentsService, private cdr: ChangeDetectorRef, private fb: FormBuilder,)
    {
		this.form = this.fb.group({
			userName: ['', [Validators.required, Validators.maxLength(50)]],
			email: ['', [Validators.required, Validators.email]],
			text: ['', [Validators.required]],
			homePage: ['', [urlValidator]],
			captchaAnswer: ['', [Validators.required]],
		});
    }

	ngOnInit(): void
	{
		this.loadCaptcha();
	}

    createComment(): void
    {
		if (this.form.invalid)
        {
            this.form.markAllAsTouched();
            return;
        }

		const data =
        {
            ...this.form.value,
			userName: this.form.value.userName.trim(),
            parentId: this.parentId,
			captchaId: this.captchaId,
            files: this.files
        };
		
        this.commentsService.createComment(data).subscribe(
		{
			next: comment =>
			{
				this.form.reset();

                this.files = [];

				this.created.emit(comment);
			},

			error: err =>
            {
				this.loadCaptcha()
                if (err.status === 400 && err.error?.errors)
                {
                    this.applyBackendErrors(
                        err.error.errors);
                }
            }
        });
    }

	applyBackendErrors(errors: any): void
	{
		for (const key in errors)
		{
			if (key.startsWith('Files['))
			{
				const messages = errors[key];
				const match = key.match(/Files\[(\d+)\]/);

				if (match)
				{
					const index = Number(match[1]);

					this.fileErrors =
					{
						...this.fileErrors,

						[index]: messages[0]
					};
				}

				continue;
			}

			const controlName =	key.charAt(0).toLowerCase()	+ key.slice(1);

			const control =	this.form.get(controlName);

			if (control)
			{
				control.setErrors({
					backend: errors[key][0]
				});
			}
		}
	}

	loadCaptcha(): void
	{
		this.commentsService.getCaptcha().subscribe(response =>
		{
			this.captchaId = response.headers.get('Captcha-Id') ?? 'm';

			const blob = response.body!;

			this.captchaImage =	URL.createObjectURL(blob);
			this.cdr.detectChanges();
		});
	}

	openFileDialog(): void
	{
		this.fileSelecting.emit(true);
	}

	onFilesSelected(event: Event): void
	{
		setTimeout(() =>
		{
			this.fileSelecting.emit(false);
		}, 300);
		
		const input = event.target as HTMLInputElement;

		this.fileErrors = []
		if (!input.files)
			return;

		this.files = Array.from(input.files);
	}

	wrapTag(tag: string): void
	{
		const textarea = this.textArea.nativeElement;

		const start = textarea.selectionStart;

		const end =	textarea.selectionEnd;

		const text = this.form.controls['text'].value ?? '';

		const selected = text.substring(start, end);

		const replacement =  `<${tag}>${selected}</${tag}>`;

		const newText =
			text.substring(0, start) +
			replacement +
			text.substring(end);

		this.form.controls['text'].setValue(newText);
	}

	insertLink(): void
	{
		const textarea = this.textArea.nativeElement;

		const start = textarea.selectionStart;

		const end = textarea.selectionEnd;

		const text = this.form.controls['text'].value ?? '';

		const selected = text.substring(start, end);

		const replacement = `<a href="" title="">${selected}</a>`;

		const newText =
			text.substring(0, start) +
			replacement +
			text.substring(end);

		this.form.controls['text'].setValue(newText);
	}
}