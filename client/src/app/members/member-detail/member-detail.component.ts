import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/members';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;
  member: Member = {} as Member;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];

  constructor(private memberService: MembersService, 
    private route: ActivatedRoute, private messageService: MessageService) { }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => this.member = data['member']
    })
    this.initializeGalleryOptions();

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tabs'])
      }
    })

    this.galleryImages = this.getImages();
  }

  loadMessages() {
    if (this.member) {
      this.messageService.getMessageThread(this.member.username).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  initializeGalleryOptions() {
    this.galleryOptions = [
      {
        width: '500px',
        height: '600px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
  }

  onTabActivated(tab: TabDirective) {
    this.activeTab = tab;
    if (this.activeTab?.heading === 'Messages') {
      this.loadMessages();
    }
  }

  getImages(): NgxGalleryImage[] {
    if (!this.member || !this.member.photos) return [];

    return this.member.photos.map(photo => ({
      small: photo.url,
      medium: photo.url,
      big: photo.url
    }));
  }
}
