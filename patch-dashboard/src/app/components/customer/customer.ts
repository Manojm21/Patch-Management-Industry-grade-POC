import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-customer',
  standalone: true,
  templateUrl: './customer.html',
  styleUrls: ['./customer.css'],
  imports: [CommonModule, FormsModule]
})
export class CustomerComponent implements OnInit {
  customers: any[] = [];
  enrichedCustomers: any[] = [];
  selectedCustomer: any = null;
  selectedProducts: any[] = [];

  showOptions: boolean = false;
  currentView: string = '';
  
  newCustomer: any = {
    id: '',
    agentId: '',
    customerName: '',
    currentVersion: ''
  };

  // New properties for monitored products
  monitoredProducts: any[] = [
    { monitoredProduct: '', currentVersion: '' }
  ];

  // Available products (you can fetch this from backend or hardcode)
  availableProducts: string[] = [
    'ProductX',
    'ProductY', 
    'ProductA',
    'ProductB',
    
  ];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.fetchCustomers();
  }

  toggleOptions() {
  if (this.showOptions) {
    // If options are already shown, collapse everything
    this.showOptions = false;
    this.currentView = '';
    this.selectedCustomer = null; // also clear selected customer view if open
  } else {
    // Show options
    this.showOptions = true;
  }
}


  setView(view: string) {
    this.currentView = view;
    this.selectedCustomer = null;
    if (view === 'display') {
      this.fetchCustomers();
    } else if (view === 'add') {
      // Reset form when switching to add view
      this.resetForm();
    }
  }

  resetForm() {
    this.newCustomer = {
      id: '',
      agentId: '',
      customerName: '',
      currentVersion: ''
    };
    this.monitoredProducts = [
      { monitoredProduct: '', currentVersion: '' }
    ];
  }

  generateUniqueId(): number {
    let newId = Math.floor(Math.random() * 1000000);
    while (this.customers.find(c => c.id == newId)) {
      newId = Math.floor(Math.random() * 1000000);
    }
    return newId;
  }

  // Generate unique ID for monitored products
  generateProductId(): number {
    return Math.floor(Math.random() * 1000000);
  }

  fetchCustomers() {
    this.http.get<any[]>('http://localhost:5000/api/customeragent').subscribe(customerData => {
      this.customers = customerData;

      this.http.get<any[]>('http://localhost:5000/api/agentmonitoredproducts').subscribe(productData => {
        this.enrichedCustomers = this.customers.map(customer => {
          const matchedProducts = productData.filter(p => p.agentId === customer.agentId);

          return {
            ...customer,
            monitoredProducts: matchedProducts,
            // currentVersion: matchedProducts.length > 0 ? matchedProducts[0].currentVersion : 'N/A'
          };
        });
      });
    });
  }

  viewDetails(customer: any) {
    this.selectedCustomer = customer;
    this.http.get<any[]>('http://localhost:5000/api/agentmonitoredproducts')
      .subscribe(products => {
        this.selectedProducts = products.filter(p => p.agentId === customer.agentId);
      });
  }

  backToList() {
    this.selectedCustomer = null;
  }

  // Add more product fields
  addProductField() {
    this.monitoredProducts.push({ monitoredProduct: '', currentVersion: '' });
  }

  // Remove product field
  removeProductField(index: number) {
    if (this.monitoredProducts.length > 1) {
      this.monitoredProducts.splice(index, 1);
    }
  }

  // Validate monitored products
  validateMonitoredProducts(): boolean {
    return this.monitoredProducts.some(product => 
      product.monitoredProduct && product.monitoredProduct.trim() !== ''
    );
  }

  async addCustomer() {
    // Validate required fields
    if (!this.newCustomer.agentId || !this.newCustomer.customerName) {
      alert('Agent ID and Customer Name are required');
      return;
    }

    // Validate that at least one product is specified
    if (!this.validateMonitoredProducts()) {
      alert('At least one monitored product must be specified');
      return;
    }

    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    
    try {
      // Generate unique ID for customer
      this.newCustomer.id = this.generateUniqueId();

      // Step 1: Create the customer
      await this.http.post('http://localhost:5000/api/customeragent', this.newCustomer, { headers }).toPromise();
      
      // Step 2: Add monitored products for this customer
      const validProducts = this.monitoredProducts.filter(product => 
        product.monitoredProduct && product.monitoredProduct.trim() !== ''
      );

      for (const product of validProducts) {
        const monitoredProductData = {
          id: this.generateProductId(),
          agentId: this.newCustomer.agentId,
          monitoredProduct: product.monitoredProduct,
          currentVersion: product.currentVersion || '1.0.0' // Default version if not specified
        };

        await this.http.post('http://localhost:5000/api/agentmonitoredproducts', monitoredProductData, { headers }).toPromise();
      }

      alert('Customer and monitored products added successfully!');
      this.resetForm();
      this.setView('display');

    } catch (error) {
      console.error('Error adding customer:', error);
      alert('Failed to add customer. Check backend or data.');
    }
  }
}