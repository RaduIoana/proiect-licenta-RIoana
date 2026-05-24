import {Component, EventEmitter, Input, Output} from '@angular/core';
import {Dialog} from 'primeng/dialog';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {MessageService} from 'primeng/api';
import {Review, ReviewService} from '../../services/review.service';
import {Toast} from 'primeng/toast';
import {Textarea} from 'primeng/textarea';
import {InputText} from 'primeng/inputtext';
import {Rating} from 'primeng/rating';
import {Button} from 'primeng/button';
import {ActivatedRoute} from '@angular/router';
import {NgIf} from '@angular/common';
import {firstValueFrom} from 'rxjs';

@Component({
  selector: 'app-review-form',
  imports: [
    Dialog,
    FormsModule,
    ReactiveFormsModule,
    Textarea,
    InputText,
    Rating,
    Button,
    NgIf
  ],
  templateUrl: './review-form.component.html',
  styleUrl: './review-form.component.scss'
})
export class ReviewFormComponent {
  reviewForm: FormGroup;
  rating: number = 0;
  title: string | undefined;
  content: string | undefined;
  existing: boolean = false;
  userReview!: Review;

  @Input() display: boolean = false;
  @Output() displayChange = new EventEmitter<boolean>();

  constructor(private fb: FormBuilder, private formMessageService: MessageService,
              private route: ActivatedRoute, private reviewService: ReviewService) {
    this.reviewForm = this.fb.group({
      rating: [0, Validators.required],
      title: [''],
      content: [''],
    });
  }

  ngOnInit() {
    this.route.params.subscribe(async params => {
      const id = params['id'];
      if (id){
        this.existing = await this.reviewService.checkExistingReview(id);
        await this.fillForm(id);
      } else {
        console.log('no app found');
        this.formMessageService.add({severity: 'error', summary: 'Error', detail: 'No app found'});
      }
    })
  }

  hideDialog(): void {
    this.display = false;
    this.displayChange.emit(this.display);
  }

  review(form: any): void {
    this.route.params.subscribe(async params => {
      const id = params['id'];
      if (id) {
        if (this.reviewForm.valid) {
          if (this.existing) {
            await this.reviewService.editReview(this.reviewForm.value, id);
            this.formMessageService.add({severity: 'success', summary: 'Success', detail: 'Review edited'});
            window.location.reload();
          } else {
            await this.reviewService.review(this.reviewForm.value, id);
            this.formMessageService.add({severity: 'success', summary: 'Success', detail: 'Review posted'});
            window.location.reload();
          }
        }
      } else {
        this.formMessageService.add({severity: 'error', summary: 'Error', detail: 'No app found'});
      }
    })
  }

  deleteReview(): void {
    this.route.params.subscribe(async params => {
      const id = params['id'];
      if (id){
        await this.reviewService.deleteReview(id);
        window.location.reload();
      } else {
        console.log('no app found');
        this.formMessageService.add({severity: 'error', summary: 'Error', detail: 'No app found'});
      }
    })
  }

  async fillForm(id: number): Promise<void> {
    if (this.existing){
      this.userReview = await firstValueFrom(this.reviewService.getUserReview(id));
      this.reviewForm.patchValue({
        title: this.userReview.title,
        content: this.userReview.content,
        rating: this.userReview.rating,
      })
    }
  }
}
