import { Component } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-patch',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patch.html',
  styleUrls: ['./patch.css']
})
export class PatchComponent {
  showOptions = false;
  currentView: string = '';
  patches: any[] = [];

  selectedZipFile: File | null = null;

  newPatch: any = {
    id: '',
    patchName: '',
    description: '',
    status: 'Released',
    scheduledTime: '',
    targetType: 0,
    productName: '',
    version: '',
    downloadUrl: 'https://example.com/patches/',
  };

  productname: string = '';
  patchname: string = '';

  constructor(private http: HttpClient) {}

  toggleOptions() {
  if (this.showOptions) {
    // If already open, close everything
    this.showOptions = false;
    this.currentView = '';
  } else {
    // Open options
    this.showOptions = true;
  }
}


  setView(view: string) {
    this.currentView = view;
    if (view === 'display') this.fetchPatches();
  }

  fetchPatches() {
    this.http.get<any[]>('http://localhost:5000/api/patch').subscribe(data => {
      this.patches = data;
    });
  }

  generateUniqueId(): number {
    let newId = Math.floor(Math.random() * 1000000);
    while (this.patches.find(p => p.id == newId)) {
      newId = Math.floor(Math.random() * 1000000);
    }
    return newId;
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      // Validate file type
      if (!file.name.toLowerCase().endsWith('.zip')) {
        alert('Please select a .zip file');
        return;
      }
      
      // Check file size (e.g., max 100MB)
      const maxSizeInMB = 100;
      if (file.size > maxSizeInMB * 1024 * 1024) {
        alert(`File size must be less than ${maxSizeInMB}MB`);
        return;
      }
      
      this.selectedZipFile = file;
      console.log('Selected file:', file.name, 'Size:', file.size);
    } else {
      this.selectedZipFile = null;
    }
  }

  submitPatch() {
    // Validate required fields
    if (!this.newPatch.productName || !this.newPatch.patchName) {
      alert('Product Name and Patch Name are required!');
      return;
    }

    // Generate unique ID
    this.newPatch.id = this.generateUniqueId();

    // Update download URL
    this.newPatch.downloadUrl = `https://example.com/patches/${this.newPatch.productName}/${this.newPatch.patchName}/installer.zip`;

    console.log('Submitting patch:', this.newPatch);

    // Send patch data first
    this.http.post('http://localhost:5000/api/patch', this.newPatch).subscribe({
      next: (response) => {
        console.log('Patch created successfully:', response);
        alert('Patch added successfully!');

        // Upload zip file after successful patch creation
        if (this.selectedZipFile) {
          this.uploadZipFile();
        } else {
          console.log('No zip file to upload');
          this.setView('display');
        }
      },
      error: (error: HttpErrorResponse) => {
        console.error('Error creating patch:', error);
        alert('Failed to create patch: ' + (error.error?.message || error.message));
      }
    });
  }

  private uploadZipFile() {
    if (!this.selectedZipFile || !this.newPatch.productName || !this.newPatch.patchName) {
      console.error('Missing required data for zip upload');
      return;
    }

    const formData = new FormData();
    
    // Log what we're about to send
    console.log('Uploading zip file:', {
      fileName: this.selectedZipFile.name,
      fileSize: this.selectedZipFile.size,
      productName: this.newPatch.productName,
      patchName: this.newPatch.patchName
    });

    // Append file with proper key name (matches your DTO property)
    formData.append('PatchZip', this.selectedZipFile);
    formData.append('ProductName', this.newPatch.productName);
    formData.append('PatchName', this.newPatch.patchName);

    // Log FormData contents (for debugging)
    console.log('FormData contents:');
    formData.forEach((value, key) => {
      console.log(key, value);
    });

    this.http.post('http://localhost:5000/api/patch/uploadZip', formData).subscribe({
      next: (response) => {
        console.log('Zip upload successful:', response);
        alert('Zip file uploaded successfully!');
        this.setView('display');
      },
      error: (error: HttpErrorResponse) => {
        console.error('Zip upload failed:', error);
        
        // More detailed error reporting
        let errorMessage = 'Zip file upload failed!';
        if (error.status === 400) {
          errorMessage += ' Bad Request - Check file format and required fields.';
        } else if (error.status === 500) {
          errorMessage += ' Server Error - Check server logs and directory permissions.';
        } else if (error.status === 0) {
          errorMessage += ' Network Error - Check if server is running and CORS is configured.';
        } else {
          errorMessage += ` HTTP ${error.status}: ${error.error?.message || error.message}`;
        }
        
        alert(errorMessage);
        console.log('Full error object:', error);
        
        // Still navigate to display even if zip upload fails
        this.setView('display');
      }
    });
  }

  deletePatch() {
    if (!this.productname || !this.patchname) {
      alert('Please enter both Product Name and Patch Name');
      return;
    }

    this.http.delete(`http://localhost:5000/api/patch/product/${this.productname}/patch/${this.patchname}`).subscribe({
      next: (response) => {
        console.log('Patch deleted successfully:', response);
        alert('Patch deleted successfully!');
        this.setView('display');
      },
      error: (error: HttpErrorResponse) => {
        console.error('Delete patch failed:', error);
        alert('Failed to delete patch: ' + (error.error?.message || error.message));
      }
    });
  }
}