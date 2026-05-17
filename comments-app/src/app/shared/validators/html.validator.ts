import { AbstractControl, ValidationErrors } from '@angular/forms';


const allowed = /<\/?(b|i|code)>/gi;

export function htmlValidator(control: AbstractControl) : ValidationErrors | null
{
    const value = control.value;

    if (!value) return null;

    const cleaned = value.replace(allowed, '');

    const hasOtherTags = /<[^>]+>/g.test(cleaned);

    return hasOtherTags ? { invalidHtml: true } : null;
}