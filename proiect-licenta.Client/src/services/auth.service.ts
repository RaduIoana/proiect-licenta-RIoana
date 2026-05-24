import { Injectable } from '@angular/core';
import {Router} from '@angular/router';
import {jwtDecode} from 'jwt-decode';

interface JwtPayload {
  name: string;
  userId: string;
  password: string;
  role: string[];
  expires: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private router: Router) {}

  getUsername(): string{
    const payload = this.decodeToken();
    if (payload == null)
      return 'Guest';
    return payload.name;
  }

  getRoles(): string[] {
    const payload = this.decodeToken();
    if (payload == null) {
      return [];
    }
    return payload.role;
  }

  hasRole(role: string): boolean {
    const payload = this.decodeToken();
    if (payload == null) {return false;}
    return payload.role.includes(role);
  }

  hasAnyRole(roles: string[]): boolean {
    const payload = this.decodeToken();
    if (payload == null) {return false;}
    return roles.some(role => payload.role.includes(role));
  }

  canAccess(requiredRoles: string[]): boolean {
    if (!this.hasAnyRole(requiredRoles)) {
      this.router.navigate(['/unauthorized']);
      return false;
    }
    return true;
  }

  isLoggedIn(): boolean {
    return !!sessionStorage.getItem("accessToken");
  }

  decodeToken(): JwtPayload | null {
    const token = sessionStorage.getItem('accessToken');
    if (token) {
      try {
        return jwtDecode<JwtPayload>(token);
      }
      catch (error) {
        console.log("Error when decoding JWT token:", error);
        return null;
      }
    }
    return null;
  }

}
