import {ActivatedRouteSnapshot, CanActivate, CanActivateFn, Router, RouterStateSnapshot} from '@angular/router';
import {LoginService} from './login.service';
import {AuthService} from './auth.service';
export class AuthGuard implements CanActivate {
  constructor(
    private loginService: LoginService,
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    const requiredRoles = route.data['roles'] as string[];

    if (!this.loginService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }

    return this.authService.canAccess(requiredRoles);
  }
}
