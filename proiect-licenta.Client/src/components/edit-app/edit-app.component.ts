import { Component } from '@angular/core';
import {Button} from "primeng/button";
import {InputText} from "primeng/inputtext";
import {MultiSelect} from "primeng/multiselect";
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {Toast} from "primeng/toast";
import {CategoriesService, Category} from '../../services/categories.service';
import {App, AppImage, AppsService} from '../../services/apps.service';
import {MessageService} from 'primeng/api';
import {firstValueFrom} from 'rxjs';
import {ActivatedRoute} from '@angular/router';
import {FileUpload} from 'primeng/fileupload';

@Component({
  selector: 'app-edit-app',
  imports: [
    Button,
    InputText,
    MultiSelect,
    ReactiveFormsModule,
    Toast,
    FileUpload
  ],
  templateUrl: './edit-app.component.html',
  styleUrl: './edit-app.component.scss'
})
export class EditAppComponent {
  app!: App;
  appForm: FormGroup;
  name: string | undefined;
  description: string | undefined;
  price: number | undefined;

  availableCategories: Category[] = [];
  currentCategories: number[] = [];
  disableButton: boolean = false;

  icon: File | null = null;
  screenshots: File[] = [];
  exe: File | null = null;

  constructor(private fb: FormBuilder, private appService: AppsService, private route: ActivatedRoute,
              private messageService: MessageService, private categoryService: CategoriesService) {
    this.appForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: ['', Validators.required],
      selectedCategories: [[] as number[]],
    });
  }

  async editIcon(id: number){
    const formData = new FormData();
    if (this.icon)
      formData.append('files', this.icon);
    await this.appService.editAppIcon(id, formData);
  }

  async editScreenshots(id: number){
    const formData = new FormData();
    this.screenshots.forEach(file => {
      formData.append('files', file);
    });
    await this.appService.editAppScreenshots(id, formData);
  }

  async editFile(id: number){
    const formData = new FormData();
    if (this.exe)
      formData.append('file', this.exe);
    console.log(this.exe);
    await this.appService.postAppFile(id, formData);
  }

  ngOnInit() {
    this.route.params.subscribe(async params => {
      const id = params['id'];
      if (id){
        this.app = await firstValueFrom(this.appService.getAppById(id));

        this.availableCategories = await firstValueFrom(this.categoryService.getAllCategories());

        const cat = await firstValueFrom(this.categoryService.getAppCategories(id));
        this.currentCategories = cat.map(c => c.id);

        await this.fillForm(id);
      } else {
        console.log('no app found');
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'No app found'});
      }
    })
  }

  async editApp(form:any) {
    if(this.appForm.valid){
      this.disableButton = true;
      try {
        const app: App = {
          id: this.app.id,
          devId: this.app.devId,
          name: this.appForm.value.name,
          description: this.appForm.value.description,
          price: this.appForm.value.price,
          launchDate: this.app.launchDate,
          rating: this.app.rating,
          discount: this.app.discount
        }

        const selectedCategories: number[] = this.appForm.value.selectedCategories;
        await this.appService.editApp(app, selectedCategories);

        if (this.icon != null)
          await this.editIcon(app.id);
        if (this.screenshots.length != 0)
          await this.editScreenshots(app.id);
        if (this.exe != null)
          await this.editFile(app.id);

        form.reset();
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'App edited successfully' });
        this.disableButton = false;
        window.location.reload();
      } catch(err){
        console.log("Error creating app:" + err);
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error creating app'});
        this.disableButton = false;
      }
    }
  }

  async fillForm(id: number): Promise<void> {
    this.appForm.patchValue({
      name: this.app.name,
      description: this.app.description,
      price: this.app.price,
      selectedCategories: this.currentCategories
    })
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
    console.log(this.exe);
  }

  onFileRemove() {
    this.exe = null;
  }

}
