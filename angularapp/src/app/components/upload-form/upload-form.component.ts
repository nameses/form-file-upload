import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { fileTypeValidator } from 'src/utils/fileTypeValidator';
import { environment as env } from 'src/environments/environment';

@Component({
  selector: 'app-upload-form',
  templateUrl: './upload-form.component.html',
  styleUrls: [],
})
export class UploadFormComponent {
  title: string = 'Upload Form';
  form!: FormGroup;
  loading = false;
  error?: string;
  success: boolean = false;

  private uploadedFile!: File;

  constructor(
    private http: HttpClient,
    private formBuilder: FormBuilder,
    private router: Router
  ) {}

  get email() {
    return this.form.controls['email'];
  }
  get username() {
    return this.form.controls['username'];
  }
  get file() {
    return this.form.controls['file'];
  }

  ngOnInit() {
    this.form = this.formBuilder.group({
      username: ['', Validators.required],
      email: [
        '',
        [
          Validators.required,
          Validators.pattern('^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$'),
        ],
      ],
      file: ['', [Validators.required, fileTypeValidator(['docx'])]],
    });
  }

  onFileSelected(event: any) {
    this.uploadedFile = <File>event.target.files[0];
  }

  onSubmit(): void {
    if (this.form.invalid) {
      console.log('Form is invalid');
      return;
    }

    if (this.form.valid) {
      const formData = new FormData();
      formData.append('username', this.username.value);
      formData.append('email', this.email.value);
      formData.append('file', this.uploadedFile, this.uploadedFile.name);

      this.http.post(env.baseApiUrl + '/azure/fileupload', formData).subscribe({
        next: (response) => {
          console.log('File uploaded successfully', response);
          this.success = true;
        },
        error: (error) => {
          console.error('File upload error', error);
        },
      });
    }
  }
}
