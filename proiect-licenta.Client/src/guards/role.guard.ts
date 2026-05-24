import {CanActivate, Router} from '@angular/router';
import {AuthService} from '../services/auth.service';
import {Injectable} from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: any): boolean {
    const requiredRoles: string[] = route.data['roles'] || [];
    const hasRole = this.authService.hasAnyRole(requiredRoles);

    if (!this.authService.isLoggedIn() || !hasRole) {
      this.router.navigate(['/unauthorized']);
      return false;
    }
    return true;
  }
}
