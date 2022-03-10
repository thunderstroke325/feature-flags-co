import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class UserService {

  baseUrl: string = `${environment.url}/api/v2/user`

  constructor(private http: HttpClient) { }

  registerByEmail(email: string, password: string) {
    return this.http.post(`${this.baseUrl}/register-by-email`, { email, password });
  }

  registerByPhone(phoneNumber: string, code: string, password: string) {
    return this.http.post(`${this.baseUrl}/register-by-phone`, { phoneNumber, code, password });
  }

  loginByPassword(identity: string, password: string) {
    return this.http.post(`${this.baseUrl}/login-by-password`, { identity, password });
  }

  loginByPhoneCode(phoneNumber: string, code: string) {
    return this.http.post(`${this.baseUrl}/login-by-phone-code`, { phoneNumber, code });
  }

  resetPassword(identity: string, code: string, newPassword: string) {
    return this.http.post(`${this.baseUrl}/reset-password`, { identity, code, newPassword });
  }

  checkIdentityExists(identity: string) {
    return this.http.get(`${this.baseUrl}/check-identity-exists?identity=${identity}`);
  }

  sendIdentityCode(identity: string, scene: string) {
    return this.http.get(`${this.baseUrl}/send-identity-code`, { params: { identity, scene } });
  }

  getProfile() {
    return this.http.get(`${this.baseUrl}/user-profile`);
  }

  updateProfile(userName: string) {
    return this.http.put(`${this.baseUrl}/user-profile`, { userName });
  }
}
