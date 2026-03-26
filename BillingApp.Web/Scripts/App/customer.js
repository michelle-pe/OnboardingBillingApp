function CustomerViewModel(getAllUrl, createUrl, updateUrl, deleteUrl) {
    var self = this;

    self.customers = ko.observableArray([]);
    self.searchTerm = ko.observable('');
    self.isLoading = ko.observable(false);
    self.alertMessage = ko.observable('');
    self.alertClass = ko.observable('alert-success');
    self.isEditing = ko.observable(false);
    self.modalError = ko.observable('');

    self.form = {
        CustomerId: ko.observable(0),
        FirstName: ko.observable(''),
        LastName: ko.observable(''),
        Email: ko.observable(''),
        Phone: ko.observable(''),
        Address: ko.observable('')
    };

    self.modalTitle = ko.computed(function () {
        return self.isEditing() ? 'Edit Customer' : 'New Customer';
    });

    self.saveButtonText = ko.computed(function () {
        return self.isEditing() ? 'Update' : 'Create';
    });

    self.filteredCustomers = ko.computed(function () {
        var term = self.searchTerm().toLowerCase();
        if (!term) return self.customers();
        return ko.utils.arrayFilter(self.customers(), function (c) {
            return (c.FirstName() + ' ' + c.LastName()).toLowerCase().indexOf(term) > -1
                || c.Email().toLowerCase().indexOf(term) > -1;
        });
    });

    self.loadCustomers = function () {
        self.isLoading(true);
        $.get(getAllUrl, function (res) {
            if (res.success) {
                var mapped = res.data.map(function (c) {
                    return {
                        CustomerId: ko.observable(c.CustomerId),
                        FirstName: ko.observable(c.FirstName),
                        LastName: ko.observable(c.LastName),
                        Email: ko.observable(c.Email),
                        Phone: ko.observable(c.Phone),
                        Address: ko.observable(c.Address)
                    };
                });
                self.customers(mapped);
            }
            self.isLoading(false);
        });
    };

    self.openCreateModal = function () {
        self.isEditing(false);
        self.modalError('');
        self.form.CustomerId(0);
        self.form.FirstName('');
        self.form.LastName('');
        self.form.Email('');
        self.form.Phone('');
        self.form.Address('');
        $('#customerModal').modal('show');
    };

    self.openEditModal = function (customer) {
        self.isEditing(true);
        self.modalError('');
        self.form.CustomerId(customer.CustomerId());
        self.form.FirstName(customer.FirstName());
        self.form.LastName(customer.LastName());
        self.form.Email(customer.Email());
        self.form.Phone(customer.Phone());
        self.form.Address(customer.Address());
        $('#customerModal').modal('show');
    };

    self.validateForm = function () {
        if (!self.form.FirstName()) { self.modalError('First name is required.'); return false; }
        if (!self.form.LastName()) { self.modalError('Last name is required.'); return false; }
        if (!self.form.Email()) { self.modalError('Email is required.'); return false; }

        var email = self.form.Email();
        var atPos = email.indexOf('@');
        var dotPos = email.lastIndexOf('.');
        if (atPos < 1 || dotPos < atPos + 2 || dotPos >= email.length - 1) {
            self.modalError('Please enter a valid email address.');
            return false;
        }

        self.modalError('');
        return true;
    };

    self.saveCustomer = function () {
        if (!self.validateForm()) return;

        var url = self.isEditing() ? updateUrl : createUrl;

        var data = {
            CustomerId: self.form.CustomerId(),
            FirstName: self.form.FirstName(),
            LastName: self.form.LastName(),
            Email: self.form.Email(),
            Phone: self.form.Phone(),
            Address: self.form.Address()
        };

        $.post(url, data, function (res) {
            if (res.success) {
                $('#customerModal').modal('hide');
                self.showAlert(res.message, 'alert-success');
                self.loadCustomers();
            } else {
                self.modalError(res.message);
            }
        });
    };

    self.deleteCustomer = function (customer) {
        if (!confirm('Delete ' + customer.FirstName() + ' ' + customer.LastName() + '?')) return;

        $.post(deleteUrl, { id: customer.CustomerId() }, function (res) {
            if (res.success) {
                self.showAlert(res.message, 'alert-success');
                self.loadCustomers();
            } else {
                self.showAlert(res.message, 'alert-danger');
            }
        });
    };

    self.showAlert = function (message, cssClass) {
        self.alertClass(cssClass);
        self.alertMessage(message);
        setTimeout(function () { self.alertMessage(''); }, 4000);
    };

    self.loadCustomers();
}