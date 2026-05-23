import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { ProductDto, ProductsService } from 'src/app/shared/services/products.service';
import { TicketsService, TicketDto, UpdateTicketRequest } from 'src/app/shared/services/tickets.service';
import { CommentsService, CommentReadDto } from 'src/app/shared/services/comments.service';
import { AttachmentsService, AttachmentDto } from 'src/app/shared/services/attachments.service';
import { UsersService, UserDto } from 'src/app/shared/services/users.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { environment } from 'src/environments/environment';
@Component({
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styles: []
})
export class TicketDetailsComponent implements OnInit {
  ticketId: string = '';
  isLoading = false;
  errorMessage = '';
  products: ProductDto[] = [];
  employees: UserDto[] = [];
  showEditModal = false;
  isClient = false;
  isManager = false;
  isEmployee = false;
  showAssignModal = false;
  editForm!: FormGroup;
  isSaving = false;
  editSubmitted = false;
  editErrorMessage = '';
  statusChangeLoading = false;
  statusChangeErrorMessage = '';
  selectedEmployeeId = '';
  assignLoading = false;
  assignErrorMessage = '';
  priority: string = 'High';
  relatedAssets: Array<{ id: string; name: string }> = [];
  estimatedResolution: string | null = null;
  ticket: (TicketDto & { description?: string; createdDate?: string; productId?: string; ProductId?: string }) | null = null;
  comments: CommentReadDto[] = [];
  newCommentText = '';
  commentsLoading = false;
  commentSubmitting = false;
  commentError = '';

  constructor(
    private activatedRoute: ActivatedRoute,
    private ticketsService: TicketsService,
    private productsService: ProductsService,
    private commentsService: CommentsService,
    private attachmentsService: AttachmentsService,
    private usersService: UsersService,
    private toastr: ToastrService,
    private authService: AuthService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.activatedRoute.paramMap.subscribe(params => {
      this.ticketId = params.get('id') ?? '';
      if (this.ticketId) {
        this.loadTicketDetails();
      }
    });
    this.isClient = this.authService.isClient();
    this.isManager = this.authService.isManager();
    this.isEmployee = this.authService.isEmployee();
  }

  getInitials(name?: string): string {
    if (!name) return 'U';
    try {
      return name.split(' ').map(n => n.charAt(0)).join('').slice(0, 2).toUpperCase();
    } catch {
      return name.charAt(0).toUpperCase();
    }
  }

  getRoleBadgeClass(role?: string): string {
    if (!role) return 'badge bg-light text-dark border';
    const r = role.toLowerCase();
    if (r.includes('manager')) return 'badge bg-warning text-dark';
    if (r.includes('employee')) return 'badge bg-info text-dark';
    if (r.includes('client')) return 'badge bg-secondary text-white';
    return 'badge bg-light text-dark border';
  }

  private loadTicketDetails(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.ticketsService.getTicketById(this.ticketId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: t => {
          this.ticket = t;
          console.log('Ticket Status:', this.ticket?.status);
          this.loadAttachments();
          this.loadProducts();
          this.loadEmployees();
        },
       
        error: err => {
          console.error('Failed to load ticket', err);
          this.errorMessage = err?.error?.message ||'Failed to load ticket details.';
        }
      });
  }

  private loadProducts(): void {
    this.productsService.getAllProducts('', 'name-asc', 1, 1000)
      .subscribe({
        next: response => {
          this.products = response.data ?? [];
          this.syncEditFormWithTicket();
          this.loadComments();
        },
        error: err => {
          console.error('Failed to load products', err);
          this.products = [];
        }
      });
  }

  private loadComments(): void {
    if (!this.ticketId) return;
    this.commentsLoading = true;
    this.comments = [];
    this.commentsService.getComments(this.ticketId).pipe(finalize(() => this.commentsLoading = false)).subscribe({
      next: res => {
        this.comments = (res.data ?? []).map(c => ({
          id: c.id,
          text: c.text,
          createdAt: c.createdAt,
          userName: c.userName,
          userRole: c.userRole,
          userId: c.userId
        }));
      },
      error: err => {
        console.error('Failed to load comments', err);
      }
    });
  }

  private loadAttachments(): void {
    if (!this.ticketId) return;

    this.attachmentsService.getAttachmentsByTicket(this.ticketId).subscribe({
      next: response => {
        const attachments = response.data ?? [];
        this.relatedAssets = attachments.map((attachment: AttachmentDto) => ({
          id: attachment.id,
          name: attachment.fileName,
        }));
      },
      error: err => {
        console.error('Failed to load attachments', err);
        this.relatedAssets = [];
      }
    });
  }

  downloadAttachment(attachment: { id: string; name: string }): void {
    this.attachmentsService.downloadAttachment(attachment.id).subscribe({
      next: blob => {
        const objectUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = objectUrl;
        link.download = attachment.name;
        link.click();
        window.URL.revokeObjectURL(objectUrl);
      },
      error: err => {
        console.error('Failed to download attachment', err);
        this.toastr.error('Failed to download attachment.', 'Error');
      }
    });
  }

  sendComment(): void {
    if (!this.ticketId) return;
    const text = (this.newCommentText || '').trim();
    if (!text) return;
    this.commentSubmitting = true;
    this.commentError = '';
    const payload = { text, ticketId: this.ticketId };
    this.commentsService.createComment(payload).pipe(finalize(() => this.commentSubmitting = false)).subscribe({
      next: res => {
        if (res?.success === false) {
          this.commentError = res?.message || 'Failed to post comment.';
          return;
        }
        this.newCommentText = '';
        this.loadComments();
      },
      error: err => {
        console.error('Failed to create comment', err);
        this.commentError = err?.error?.message || 'Failed to post comment.';
      }
    });
  }

  private loadEmployees(): void {
    this.usersService.getAllUsers('', 'name-asc', 'Employee', true, 1, 1000)
      .subscribe({
        next: response => this.employees = response.data ?? [],
        error: err => {
          console.error('Failed to load employees', err);
          this.employees = [];
        }
      });
  }

  private getTicketProductId(): string {
    const rawProductId = this.ticket?.productId ?? this.ticket?.ProductId ?? '';
    if (rawProductId) {
      return rawProductId;
    }

    const matchedProduct = this.products.find(product => product.name === this.ticket?.productName);
    return matchedProduct?.id ?? '';
  }

  private syncEditFormWithTicket(): void {
    if (!this.ticket || !this.editForm) {
      return;
    }

    const productId = this.getTicketProductId();
    if (productId) {
      this.editForm.patchValue({ productId });
    }
  }

  openEditModal(): void {
    if (!this.ticket) {
      return;
    }

    this.editErrorMessage = '';
    this.editSubmitted = false;

    this.editForm = this.fb.group({
      title: [this.ticket.title || '', [Validators.required, Validators.minLength(3)]],
      description: [this.ticket.description || '', [Validators.required, Validators.minLength(10)]],
      productId: [this.getTicketProductId(), Validators.required]
    });

    this.showEditModal = true;

    if (!this.products.length) {
      this.loadProducts();
    }
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editErrorMessage = '';
    this.editSubmitted = false;
  }

  onEditModalBackdropClick(): void {
    if (!this.isSaving) {
      this.closeEditModal();
    }
  }

  saveEdit(): void {
    if (!this.ticket || !this.editForm) {
      return;
    }

    this.editSubmitted = true;
    this.editErrorMessage = '';

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.editErrorMessage = 'Please fix the validation errors before saving.';
      this.toastr.error(this.editErrorMessage, 'Validation Error');
      return;
    }

    this.isSaving = true;
    const values = this.editForm.value;
    const payload: UpdateTicketRequest = {
      title: (values.title ?? '').trim(),
      description: (values.description ?? '').trim(),
      productId: values.productId
    };

    this.ticketsService.updateTicket(this.ticketId, payload)
      .pipe(finalize(() => this.isSaving = false))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.editErrorMessage = response?.message || 'Failed to update ticket.';
            this.toastr.error(this.editErrorMessage, 'Error');
            return;
          }

          this.toastr.success(response?.message || 'Ticket updated successfully.', 'Success');
          this.closeEditModal();
          this.loadTicketDetails();
        },
        error: err => {
          console.error('Failed to update ticket', err);
          this.editErrorMessage = err?.error?.message || 'Failed to update ticket.';
          this.toastr.error(this.editErrorMessage, 'Error');
        }
      });
  }

  openAssignModal(): void {
    if (!this.ticket) {
      return;
    }

    this.assignErrorMessage = '';
    this.selectedEmployeeId = '';
    this.showAssignModal = true;
    if (!this.employees.length) {
      this.loadEmployees();
    }
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.assignErrorMessage = '';
    this.selectedEmployeeId = '';
  }

  onAssignModalBackdropClick(): void {
    if (!this.assignLoading) {
      this.closeAssignModal();
    }
  }

  assignTicket(): void {
    if (!this.ticket || !this.selectedEmployeeId) {
      this.assignErrorMessage = 'Please choose an employee to assign.';
      return;
    }

    this.assignLoading = true;
    this.assignErrorMessage = '';

    this.ticketsService.assignTicketToEmployee(this.ticketId, this.selectedEmployeeId)
      .pipe(finalize(() => this.assignLoading = false))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.assignErrorMessage = response?.message || 'Failed to assign ticket.';
            this.toastr.error(this.assignErrorMessage, 'Error');
            return;
          }

          this.toastr.success(response?.message || 'Ticket assigned successfully.', 'Success');
          this.closeAssignModal();
          this.loadTicketDetails();
        },
        error: err => {
          console.error('Failed to assign ticket', err);
          this.assignErrorMessage = err?.error?.message || 'Failed to assign ticket.';
          this.toastr.error(this.assignErrorMessage, 'Error');
        }
      });
  }

  changeTicketStatus(): void {
    if (!this.ticket || this.statusChangeLoading) {
      return;
    }

    this.statusChangeLoading = true;
    this.statusChangeErrorMessage = '';

    this.ticketsService.changeTicketStatus(this.ticketId)
      .pipe(finalize(() => this.statusChangeLoading = false))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.statusChangeErrorMessage = response?.message || 'Failed to update ticket status.';
            this.toastr.error(this.statusChangeErrorMessage, 'Error');
            return;
          }

          this.toastr.success(response?.message || 'Ticket status updated successfully.', 'Success');
          this.loadTicketDetails();
        },
        error: err => {
          console.error('Failed to change ticket status', err);
          this.statusChangeErrorMessage = err?.error?.message || 'Failed to update ticket status.';
          this.toastr.error(this.statusChangeErrorMessage, 'Error');
        }
      });
  }

  markAsFixed(): void {
    if (!this.ticket || this.isSaving) return;

    this.isSaving = true;

    this.ticketsService.fixTicket(this.ticketId)
      .pipe(finalize(() => this.isSaving = false))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            const msg = response?.message || 'Failed to mark ticket as fixed.';
            this.toastr.error(msg, 'Error');
            return;
          }

          this.toastr.success(response?.message || 'Ticket marked as fixed.', 'Success');
          this.loadTicketDetails();
        },
        error: err => {
          console.error('Failed to mark ticket as fixed', err);
          const msg = err?.error?.message || 'Failed to mark ticket as fixed.';
          this.toastr.error(msg, 'Error');
        }
      });
  }

  getStatusBadgeClass(): string {
    const status = (this.ticket?.status || '').toLowerCase();
    if (status.includes('progress')) return 'bg-info';
    if (status.includes('resolved') || status.includes('fixed')) return 'bg-success';
    if (status.includes('new')) return 'bg-primary';
    if (status.includes('closed')) return 'bg-secondary';
    return 'bg-light text-dark border';
  }

  getStatusIcon(): string {
    const status = (this.ticket?.status || '').toLowerCase();
    if (status.includes('progress')) return 'bi-hourglass-split';
    if (status.includes('resolved') || status.includes('fixed')) return 'bi-check-circle-fill';
    if (status.includes('new')) return 'bi-exclamation-circle-fill';
    if (status.includes('closed')) return 'bi-x-circle-fill';
    return 'bi-circle';
  }
}
