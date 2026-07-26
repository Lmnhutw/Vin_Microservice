import { Injectable, signal } from '@angular/core';

export type NotificationTone = 'success' | 'error' | 'info';

export interface AppNotification {
  id: number;
  message: string;
  tone: NotificationTone;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 1;
  readonly notification = signal<AppNotification | null>(null);
  private dismissTimer?: ReturnType<typeof setTimeout>;

  show(message: string, tone: NotificationTone = 'info'): void {
    if (this.dismissTimer) clearTimeout(this.dismissTimer);
    this.notification.set({ id: this.nextId++, message, tone });
    this.dismissTimer = setTimeout(() => this.dismiss(), 4200);
  }

  dismiss(): void {
    if (this.dismissTimer) clearTimeout(this.dismissTimer);
    this.notification.set(null);
  }
}
