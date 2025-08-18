import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-status',
  standalone: true,
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.css'],
  imports: [CommonModule, FormsModule]
})
export class StatusComponent implements OnInit {
  statuses: any[] = [];
  searchText: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.fetchStatuses();
  }

  fetchStatuses() {
    this.http.get<any[]>('http://localhost:5000/api/status/reports')
      .subscribe(
        data => {
          this.statuses = data.map(item => ({
            agentId: item.agentId,
            product: item.productName,
            version: item.patchVersion,
            status: this.getStatusLabel(item.status),
            time: new Date(item.timestamp).toLocaleString()
          }));
        },
        error => {
          console.error('Failed to fetch statuses:', error);
        }
      );
  }

  getStatusLabel(code: number): string {
    switch (code) {
      case 0: return 'Unapplied';
      case 1: return 'InProcess';
      case 2: return 'Applying';
      case 3: return 'Applied';
      default: return 'Failed';
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Applied':
        return 'badge badge-success';
      case 'Failed':
        return 'badge badge-error';
      case 'Applying':
      case 'InProcess':
        return 'badge badge-primary-medium';
      default:
        return 'badge badge-secondary';
    }
  }

  filteredStatuses() {
    if (!this.searchText) return this.statuses;
    const search = this.searchText.toLowerCase();
    return this.statuses.filter(s =>
      String(s.agentId).toLowerCase().includes(search) ||
      String(s.product).toLowerCase().includes(search) ||
      String(s.status).toLowerCase().includes(search) ||
      String(s.version).toLowerCase().includes(search) ||
      String(s.time).toLowerCase().includes(search)
    );
  }
}
