import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/members';
import { map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  member: Member[] = [];

  constructor(private http: HttpClient) { }

  getMembers() {
    if (this.member.length > 0) return of(this.member);
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.member = members;
        return members;
      })
    )
  }

  getMember(username: string) {
    const members = this.member.find(x => x.username === username);
    if (members) return of(members);
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.member.indexOf(member);
        this.member[index] = {...this.member[index], ...member}
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'user/delete-photo/' + photoId);
  }
}
