import {Component, EventEmitter, Input, Output} from '@angular/core';
import {Button} from "primeng/button";
import {Dialog} from "primeng/dialog";
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {InputText} from "primeng/inputtext";
import {Toast} from "primeng/toast";
import {LoginService} from '../../services/login.service';
import {MessageService} from 'primeng/api';
import {AuthService} from '../../services/auth.service';
import {CategoriesService} from '../../services/categories.service';

@Component({
  selector: 'app-create-category',
    imports: [
        Button,
        Dialog,
        FormsModule,
        InputText,
        ReactiveFormsModule,
        Toast
    ],
  templateUrl: './create-category.component.html',
  styleUrl: './create-category.component.scss'
})
export class CreateCategoryComponent {
  categoryForm: FormGroup;
  name: string | undefined;

  @Input() display: boolean = false;
  @Output() displayChange = new EventEmitter<boolean>();

  constructor(private fb: FormBuilder, private messageService: MessageService,
              private categoriesService: CategoriesService) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required]],
    });
  }

  hideDialog(): void {
    this.display = false;
    this.displayChange.emit(this.display);
  }

  async createCategory(form:any){
    if (this.categoryForm.valid) {
      await this.categoriesService.createCategory(this.categoryForm.value.name);
      window.location.reload();
    } else {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please fill out the form correctly.' });
    }
  }
}
