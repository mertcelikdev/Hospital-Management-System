// User creation form JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Form elements
    const roleSelect = document.getElementById('Role');
    const departmentDiv = document.getElementById('departmentDiv');
    const form = document.getElementById('userForm');
    
    // Role change handler
    roleSelect.addEventListener('change', function() {
        const selectedRole = this.value;
        
        if (selectedRole === 'Doctor' || selectedRole === 'Nurse') {
            departmentDiv.style.display = 'block';
            document.getElementById('Department').required = true;
        } else {
            departmentDiv.style.display = 'none';
            document.getElementById('Department').required = false;
        }
        
        // Show notification for role selection
        showNotification(`${selectedRole} rolü seçildi. Form alanları güncellendi.`, 'info');
    });
    
    // Email validation
    const emailInput = document.querySelector('input[name="Email"]');
    emailInput.addEventListener('blur', function(e) {
        const email = e.target.value;
        const emailPattern = new RegExp('^\\S+@\\S+\\.\\S+$');
        
        if (email && !emailPattern.test(email)) {
            e.target.classList.add('is-invalid');
            showNotification('Geçerli bir e-posta adresi giriniz.', 'warning');
        } else if (email) {
            e.target.classList.remove('is-invalid');
            e.target.classList.add('is-valid');
        }
    });
    
    // Phone validation
    const phoneInput = document.querySelector('input[name="Phone"]');
    phoneInput.addEventListener('input', function(e) {
        // Allow only numbers and format
        let value = e.target.value.replace(/\D/g, '');
        
        if (value.length > 0) {
            if (value.length <= 3) {
                value = value;
            } else if (value.length <= 6) {
                value = value.slice(0, 3) + ' ' + value.slice(3);
            } else {
                value = value.slice(0, 3) + ' ' + value.slice(3, 6) + ' ' + value.slice(6, 10);
            }
        }
        
        e.target.value = value;
    });
    
    // Form submission
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const formData = new FormData(form);
        const submitBtn = document.getElementById('submitBtn');
        const originalText = submitBtn.innerHTML;
        
        // Show loading state
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Kaydediliyor...';
        submitBtn.disabled = true;
        
        fetch('/User/Create', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (response.ok) {
                showNotification('Kullanıcı başarıyla oluşturuldu!', 'success');
                setTimeout(() => {
                    window.location.href = '/User';
                }, 1500);
            } else {
                return response.text().then(text => {
                    throw new Error(text);
                });
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Kullanıcı oluşturulurken bir hata oluştu.', 'error');
            
            // Reset button
            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;
        });
    });
    
    // Password strength indicator
    const passwordInput = document.querySelector('input[name="Password"]');
    const confirmPasswordInput = document.querySelector('input[name="ConfirmPassword"]');
    
    passwordInput.addEventListener('input', function(e) {
        const password = e.target.value;
        let strength = 0;
        
        if (password.length >= 8) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[a-z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[^A-Za-z0-9]/.test(password)) strength++;
        
        const strengthIndicator = document.getElementById('passwordStrength');
        if (!strengthIndicator) return;
        
        strengthIndicator.className = 'password-strength';
        
        if (strength < 2) {
            strengthIndicator.className += ' weak';
            strengthIndicator.textContent = 'Zayıf';
        } else if (strength < 4) {
            strengthIndicator.className += ' medium';
            strengthIndicator.textContent = 'Orta';
        } else {
            strengthIndicator.className += ' strong';
            strengthIndicator.textContent = 'Güçlü';
        }
    });
    
    // Confirm password validation
    confirmPasswordInput.addEventListener('blur', function(e) {
        const password = passwordInput.value;
        const confirmPassword = e.target.value;
        
        if (confirmPassword && password !== confirmPassword) {
            e.target.classList.add('is-invalid');
            showNotification('Şifreler eşleşmiyor.', 'warning');
        } else if (confirmPassword) {
            e.target.classList.remove('is-invalid');
            e.target.classList.add('is-valid');
        }
    });
});

// Notification function
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${getBootstrapAlertClass(type)} alert-dismissible fade show notification-toast`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

function getBootstrapAlertClass(type) {
    const typeMap = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning',
        'info': 'info'
    };
    return typeMap[type] || 'info';
}
