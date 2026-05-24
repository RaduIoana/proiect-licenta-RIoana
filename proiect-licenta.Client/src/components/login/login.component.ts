import {Component, EventEmitter, Input, Output} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {LoginService} from '../../services/login.service';
import {MessageService} from 'primeng/api';
import {Dialog} from 'primeng/dialog';
import {Button} from 'primeng/button';
import {Toast} from 'primeng/toast';
import {InputText} from 'primeng/inputtext';
import {AuthService} from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [
    Dialog,
    ReactiveFormsModule,
    Button,
    Toast,
    InputText
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginForm: FormGroup;
  password: string | undefined;
  email: string | undefined;

  @Input() display: boolean = false;
  @Output() displayChange = new EventEmitter<boolean>();

  constructor(private fb: FormBuilder, private loginService: LoginService,
              private messageService: MessageService, private authService: AuthService) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
    });
  }

  hideDialog(): void {
    this.display = false;
    this.displayChange.emit(this.display);
  }

  login(form:any){
    if (this.loginForm.valid) {
      this.loginService.onLogin(this.loginForm.value).subscribe(
        result => {

          sessionStorage.setItem('accessToken', result.token);
          sessionStorage.setItem('role', JSON.stringify(this.authService.getRoles()));
          sessionStorage.setItem('username', this.authService.getUsername());

          form.reset();
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Logged in successfully!' });

          window.location.reload();
        },
        (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.message || 'Login failed' });
        }
      );
    } else {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please fill out the form correctly.' });
    }
  }

}
