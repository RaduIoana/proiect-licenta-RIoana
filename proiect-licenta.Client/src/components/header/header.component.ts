import { Component } from '@angular/core';
import {Button} from 'primeng/button';
import {NgIf} from '@angular/common';
import {LoginComponent} from '../login/login.component';
import {RegisterComponent} from '../register/register.component';
import {AccountMenuComponent} from '../account-menu/account-menu.component';
import {AuthService} from '../../services/auth.service';
import {LoginService} from '../../services/login.service';
import {Router} from '@angular/router';
import {CreateCategoryComponent} from '../create-category/create-category.component';

@Component({
  selector: 'app-header',
  imports: [
    Button,
    NgIf,
    LoginComponent,
    RegisterComponent,
    AccountMenuComponent,
    CreateCategoryComponent
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  displayLogin: boolean = false;
  displayRegister: boolean = false;
  displayCreateCategory: boolean = false;
  isLoggedIn: boolean = false;
  username: string | null = "Guest";

  constructor(private authService: AuthService, private loginService: LoginService,
              private router: Router) {}

  ngOnInit() {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.username = this.authService.getUsername();
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }

  showCreateCategory(): void {
    this.displayCreateCategory = true;
  }

  showLogin(): void {
    this.displayLogin = true;
  }

  showRegister(): void {
    this.displayRegister = true;
  }

  logout(): void {
    if(this.isLoggedIn){
      this.loginService.logout();
      this.isLoggedIn = false;
    }
  }

  navigateHome() {
    this.router.navigate(['home']);
  }

  navigateLibrary() {
    this.router.navigate(['library']);
  }

  navigateRefunds(){
    this.router.navigate(['refunds/admin']);
  }

  navigateCreateApp(){
    this.router.navigate(['apps/create']);
  }

  navigateDevApp(){
    this.router.navigate(['uploaded-apps']);
  }
}
