import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function fileTypeValidator(allowedTypes: string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const file = control.value;

    if (!file) return null;

    const fileType: string = file.split('.').pop();

    if (!allowedTypes.includes(fileType)) {
      console.log('File type incorrect');
      return { fileTypeInvalid: true };
    }
    console.log('File type correct');
    return null;
  };
}
