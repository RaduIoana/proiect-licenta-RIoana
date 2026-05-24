import { Component } from '@angular/core';
import {Button} from 'primeng/button';
import {App, AppImage, AppsService} from '../../services/apps.service';
import {ActivatedRoute, Router} from '@angular/router';
import {DecimalPipe, Location, NgForOf, NgIf, NgSwitch, NgSwitchCase} from '@angular/common';
import {Rating} from 'primeng/rating';
import {FormsModule} from '@angular/forms';
import {ethers} from 'ethers';
import contractData from "../../../../hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json";
import {firstValueFrom} from 'rxjs';
import {MessageService} from 'primeng/api';
import {Toast} from 'primeng/toast';
import {LicenseService} from '../../services/license.service';
import {ReviewFormComponent} from '../review-form/review-form.component';
import {Review, ReviewService} from '../../services/review.service';
import {environment} from '../../environments/environment';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {MetaMaskService} from '../../services/metamask.service';
import {Avatar} from 'primeng/avatar';
import {Divider} from 'primeng/divider';
import {GalleriaModule} from 'primeng/galleria';
import {Carousel} from 'primeng/carousel';

@Component({
  selector: 'app-app-details',
  imports: [
    Button,
    NgSwitch,
    DecimalPipe,
    NgIf,
    NgSwitchCase,
    Rating,
    FormsModule,
    NgForOf,
    Toast,
    ReviewFormComponent,
    Avatar,
    Divider,
    GalleriaModule,
    Carousel
  ],
  templateUrl: './app-details.component.html',
  styleUrl: './app-details.component.scss'
})
export class AppDetailsComponent {

  app!: App;
  appButtonType: any;
  appInLibrary: boolean | undefined;
  images: AppImage[] = [];
  galleryIndex = 0;

  reviewFormDisplay: boolean = false;
  galleryDisplay: boolean = false;
  reviewExists: boolean = false;
  userReview: Review | null = null;
  reviews: Review[] = [];

  isAppOwner: boolean = false;
  disablePurchaseButton: boolean = false;
  disableRemoveButton: boolean = false;
  displayRemoveButton: boolean = false;

  constructor(private appService: AppsService, private route: ActivatedRoute,
              private location: Location, private router: Router,
              private messageService: MessageService, private licenseService: LicenseService,
              private reviewService: ReviewService, private metaMaskService: MetaMaskService,
              private http: HttpClient) {}

  ngOnInit() {
    this.route.params.subscribe(async params => {
      const id = params['id'];
      if (id) {
        this.app = await firstValueFrom(this.appService.getAppById(id));
        this.app.rating = await this.appService.getAppRating(id);

        try{
          this.app.icon = await this.appService.getAppIcon(id);
        } catch (error:any) {
          if (error.status === 404)
            this.app.icon = undefined;
        }
        this.appService.getAppScreenshots(id).subscribe({
          next: async (data) => {
            this.images = data;
          },
          error: (err) => {
            if (err.status === 404)
              return;
            console.error('Error fetching apps', err);
          }
        });

        this.appInLibrary = await this.appService.appIsOwnedByUser(id);
        this.isAppOwner = await this.appService.userIsAppDeveloper(id);
        this.determineButtonType();
        if (this.appButtonType == "install" && this.app.price == 0)
          this.displayRemoveButton = true;

        this.reviewExists = await this.reviewService.checkExistingReview(id);
        if (this.reviewExists)
        {
          this.reviewService.getUserReview(id).subscribe({
            next: (data) => {
              this.userReview = data;
            },
            error: (err) => {
              console.error('Error user review', err);
            }
          });
        }

        this.reviewService.getAppReviews(id).subscribe({
          next: (data) => {
            this.reviews = data;
          },
          error: (err) => {
            if (err.status === 404) {
              this.reviews = [];
              return;
            }
            console.error('Error fetching reviews', err);
          }
        });
      } else {
        console.log('no app found');
        this.messageService.add({severity: 'error', summary: 'Error', detail: 'No app found'});
      }
    })
  }

  navigateBack() {
    if(window.history.length > 1) {
      this.location.back();
    } else {
      this.router.navigate(['/home']);
    }
  }

  editApp(){
    this.router.navigate([`/apps/edit/${this.app.id}`]);
  }

  determineButtonType() {
    console.log(this.appInLibrary);
    if(this.appInLibrary)
    {
      this.appButtonType = "install";
      return;
    }
    if(this.app.price == 0 && !this.appInLibrary)
    {
      this.appButtonType = "library";
      return;
    }
    this.appButtonType = "purchase";
  }

  async addAppToLibrary() {
    if(this.app.price == 0 && !this.appInLibrary){
      await this.appService.addFreeAppToLibrary(this.app);
      this.appButtonType = "install";
      this.messageService.add({severity: 'success', summary: 'Success', detail: 'App was successfully added'});
      return;
    }
    console.log("Couldn't add app");
    this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error adding app'});
    window.location.reload();
  }

  async removeAppFromLibrary() {
    this.disableRemoveButton = true;
    try{
      if (this.app.price == 0)
      {
        await this.appService.removeAppFromLibrary(this.app.id);
        await this.appService.revokeFreeApp(this.app.id);

        console.log("Removed successfully");
        this.messageService.add({severity: 'success', detail: 'Succeeded removing app from library'});
        window.location.reload();
      } else throw new Error('App not free');
    } catch (error:any) {
      console.log("Couldn't remove app");
      this.messageService.add({severity: 'error', summary: 'Error',
        detail: 'Error removing app from library:' + error.message});
      window.location.reload();
    }
    this.disableRemoveButton = false;
  }

  async installApp() {
    try{
      await this.appService.installApp(this.app.id);
    } catch(error:any){
      console.error("Error installing app", error);
      this.messageService.add({severity: 'error', summary: 'Error', detail: 'Error installing app'});
    }
  }

  async purchaseApp() {
    try {
      this.disablePurchaseButton = true;

      // ipfs node check
      const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
      await firstValueFrom(this.http.get(`${environment.apiUrl}/api/Appstore/ipfs`, {headers}));

      const abi = contractData.abi;
      const contractAddress = environment.liveContractAddr;
      //const contractAddress = environment.localContractAddr;

      let signer = null;
      let provider;
      if ((window as any).ethereum == null) {
        console.log("MetaMask not installed, using defaults");
        provider = ethers.getDefaultProvider();
      } else {
        // for live
        provider = new ethers.BrowserProvider((window as any).ethereum);
        // for local
        //provider = new ethers.JsonRpcProvider("http://127.0.0.1:8545");

        signer = await provider.getSigner();
      }

      const existingWalletAddress = await this.metaMaskService.getWallet();
      const walletAddress = await signer!.getAddress();

      if (existingWalletAddress.toLowerCase() != walletAddress.toLowerCase()){
        this.messageService.add({severity: 'error', summary: 'Incorrect wallet used for payment, please change Metamask wallet'});
        throw new Error('Incorrect wallet address');
      }

      interface AppStoreContract {
        buyApp(appId: number, overrides?: { value?: bigint }): Promise<ethers.TransactionResponse>;
      }

      console.log("calling contract..." + this.app.id);

      const contract = new ethers.Contract(contractAddress, abi, signer) as unknown as AppStoreContract;
      const tx = await contract.buyApp(this.app.id, {value: ethers.parseEther(this.app.price.toString())});
      const receipt = await tx.wait();

      console.log("saving payment...");

      // insert record of payment
      const recordId = await this.licenseService.insertPayment(tx, this.app);

      // confirm record is valid
      if (receipt?.status === 1) {
        const paymentConfirmation = await this.licenseService.confirmPayment(recordId);
        if (paymentConfirmation.success == true) {
          //add license to chain / mint it for user
          const mintResult = await this.licenseService.mintLicense(paymentConfirmation.licenseId, recordId);

          if (mintResult.success) {
            await this.appService.addPaidAppToLibrary(this.app.id, recordId);
            this.messageService.add({severity: 'success', detail: 'App purchased successfully'});
          } else {
            this.messageService.add({severity: 'error', detail: 'Purchase failed: ' + mintResult.error});
            throw new Error('Minting failed');
          }

        } else {
          this.messageService.add({severity: 'error', summary: 'Error verifying payment'});
          throw new Error('Error verifying payment');
        }
      }
    } catch (error: any) {
      console.log("Error purchasing app:" + error.message);
      this.disablePurchaseButton = false;
      this.messageService.add({severity: 'error', summary: 'Error purchasing app:' + error.message});
      window.location.reload();
    }

    this.disablePurchaseButton = false;
    window.location.reload();
  }

  promptReviewForm() {
    this.reviewFormDisplay = true;
  }

  displayGallery(id: number){
    console.log(id);
    this.galleryDisplay = true;
    this.galleryIndex = id;
  }
}
