import {Component, EventEmitter, Input, Output} from '@angular/core';
import {Dialog} from 'primeng/dialog';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {InputText} from 'primeng/inputtext';
import {Button} from 'primeng/button';
import {Toast} from 'primeng/toast';
import {LoginService} from '../../services/login.service';
import {MessageService} from 'primeng/api';
import {Router} from '@angular/router';
import {Checkbox} from 'primeng/checkbox';

@Component({
  selector: 'app-register',
  imports: [
    Dialog,
    ReactiveFormsModule,
    InputText,
    Button,
    Toast,
    Checkbox,
    FormsModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  username: string|undefined;
  email: string|undefined;
  password: string|undefined;
  @Input() display: boolean = false;
  @Output() displayChange = new EventEmitter<boolean>();

  constructor(private fb: FormBuilder, private loginService: LoginService, private messageService: MessageService, private router:Router) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(6)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      isDeveloper: [false],
    });
  }

  hideDialog(): void {
    this.display = false;
    this.displayChange.emit(this.display);
  }

  register(form:any){
    if (this.registerForm.valid) {
      this.loginService.onRegister(this.registerForm.value).subscribe(
        result => {
          const data = result;
          form.reset();
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Registered successfully!' });
          window.location.reload();
        },
        (error) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: error.message || 'Register failed' });
        }
      );
    } else {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please fill out the form correctly.' });
    }
  }
}
