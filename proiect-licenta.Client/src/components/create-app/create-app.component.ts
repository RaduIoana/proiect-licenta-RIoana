import { Component } from '@angular/core';
import {AppsService, App} from '../../services/apps.service';
import {MessageService} from 'primeng/api';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {Toast} from 'primeng/toast';
import {Button} from 'primeng/button';
import {InputText} from 'primeng/inputtext';
import {CategoriesService, Category} from '../../services/categories.service';
import {MultiSelect} from 'primeng/multiselect';
import {FileUpload} from 'primeng/fileupload';

@Component({
  selector: 'app-create-app',
  imports: [
    ReactiveFormsModule,
    Toast,
    Button,
    InputText,
    MultiSelect,
    FileUpload
  ],
  templateUrl: './create-app.component.html',
  styleUrl: './create-app.component.scss'
})
export class CreateAppComponent {
  appForm: FormGroup;
  name: string | undefined;
  description: string | undefined;
  price: number | undefined;

  availableCategories: Category[] = [];
  disableButton: boolean = false;

  icon: File | null = null;
  screenshots: File[] = [];
  exe: File | null = null;

  constructor(private fb: FormBuilder, private appService: AppsService,
              private messageService: MessageService, private categoryService: CategoriesService) {
    this.categoryService.getAllCategories().subscribe({
      next: (data) => {
        this.availableCategories = data;
      },
      error: (err) => {
        console.error('Error fetching categories', err);
      }
    });
    this.appForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: ['', Validators.required],
      selectedCategories: [[] as number[]],
    })
  }

  async uploadIcon(id: number){
    const formData = new FormData();
    if (this.icon)
      formData.append('files', this.icon);
    await this.appService.postAppIcon(id, formData);
  }

  async uploadScreenshots(id: number){
    const formData = new FormData();
    this.screenshots.forEach(file => {
      formData.append('files', file);
    });
    await this.appService.postAppScreenshots(id, formData);
  }

  async uploadFile(id: number){
    const formData = new FormData();
    if (this.exe)
      formData.append('file', this.exe);
    await this.appService.postAppFile(id, formData);
  }

  async createApp(form:any) {
    if(this.appForm.valid){
      this.disableButton = true;
      try {
        const app: App = {
          id: 0,
          devId: "placeholder",
          name: this.appForm.value.name,
          description: this.appForm.value.description,
          price: this.appForm.value.price,
          launchDate: new Date().toISOString(),
          rating: 0,
          discount: 0
        }
        const selectedCategories: number[] = this.appForm.value.selectedCategories;
        const id = await this.appService.createApp(app, selectedCategories);

        await this.uploadIcon(id);
        await this.uploadScreenshots(id);
        await this.uploadFile(id);

        form.reset();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'App added successfully' });
        this.disableButton = false;
        //window.location.reload();
      } catch(err){
        console.log("Error creating app:" + err);
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error creating app'});
        this.disableButton = false;
      }
    }
  }

  onIconSelect(event: any) {
    this.icon = event.files[0];
  }

  onIconRemove() {
    this.icon = null;
  }

  onScreenshotSelect(event: any) {
    this.screenshots = Array.from(event.files);
  }

  onScreenshotRemove(event: any) {
    this.screenshots = this.screenshots.filter(s => s !== event.file);
  }

  onFileSelect(event: any) {
    this.exe = event.files[0];
  }

  onFileRemove() {
    this.exe = null;
  }
}


