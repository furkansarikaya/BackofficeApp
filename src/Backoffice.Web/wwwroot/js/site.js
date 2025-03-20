/**
 * Backoffice Panel - Ana JavaScript Dosyası
 *
 * Bu dosya, sol menü işlevselliği ve diğer UI etkileşimleri için
 * gerekli tüm JavaScript fonksiyonlarını içerir.
 */

/**
 * Sayfa yüklendiğinde çalışacak fonksiyonlar
 */
document.addEventListener('DOMContentLoaded', function() {
    // Sidebar toggle butonu işlevselliği
    initSidebarToggle();

    // Mini sidebar modu
    initMiniSidebar();

    // Açılır-kapanır menü öğeleri
    initCollapsibleMenus();

    // Mobil cihazlarda sidebar kapatma ve diğer responsive işlemler
    initMobileResponsive();

    // Aktif menü işlemleri
    initActiveMenuHandling();

    // Bootstrap Tooltip ve Popover'ları başlat
    initBootstrapComponents();
});

/**
 * Sidebar açma/kapatma düğmesi işlevselliği
 */
function initSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            // Sidebar'ı aç/kapat
            const sidebar = document.querySelector('.sidebar');
            const contentWrapper = document.querySelector('.content-wrapper');

            if (sidebar) sidebar.classList.toggle('active');
            if (contentWrapper) contentWrapper.classList.toggle('active');
        });
    }
}

/**
 * Mini sidebar modunu başlat
 */
function initMiniSidebar() {
    const miniToggle = document.getElementById('sidebarMiniToggle');
    if (miniToggle) {
        miniToggle.addEventListener('click', function() {
            document.body.classList.toggle('sidebar-mini');

            // Kullanıcı tercihini localStorage'da sakla
            localStorage.setItem('sidebar-mini', document.body.classList.contains('sidebar-mini'));
        });

        // Sayfa yüklendiğinde kullanıcı tercihini kontrol et
        if (localStorage.getItem('sidebar-mini') === 'true') {
            document.body.classList.add('sidebar-mini');
        }
    }
}

/**
 * Tüm açılır-kapanır menü öğelerini başlat
 */
function initCollapsibleMenus() {
    // Tüm menü öğelerini seç
    const collapsibleItems = document.querySelectorAll('.collapsible-menu-item');

    // Her menü öğesine tıklama olayı ekle
    collapsibleItems.forEach(item => {
        item.addEventListener('click', function(event) {
            // Bağlantı davranışını engelle
            event.preventDefault();

            // Hedef collapse elementini bul
            const targetId = this.getAttribute('href');
            if (!targetId) return;

            // Target elementi seç
            const targetElement = document.querySelector(targetId);
            if (!targetElement) return;

            // Mevcut durumu kontrol et
            const isExpanded = this.getAttribute('aria-expanded') === 'true';

            // Durumu güncelle
            this.setAttribute('aria-expanded', (!isExpanded).toString());

            // Bootstrap Collapse API kullan veya manuel toggle
            if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                const bsCollapse = bootstrap.Collapse.getInstance(targetElement);
                if (bsCollapse) {
                    bsCollapse.toggle();
                } else {
                    new bootstrap.Collapse(targetElement, {
                        toggle: true
                    });
                }
            } else {
                // Bootstrap yoksa manuel toggle yap
                targetElement.classList.toggle('show');
            }

            // Tıklanan menü bir bölüm başlığı mı?
            const isSectionHeader = this.classList.contains('section-header');

            // Aynı seviyedeki diğer menüleri kapat (opsiyonel)
            if (this.getAttribute('data-close-others') !== 'false') {
                closeOtherMenusInSameLevel(this, isSectionHeader);
            }
        });
    });
}

/**
 * Aynı seviyedeki diğer menüleri kapat
 * @param {HTMLElement} currentItem - Tıklanan menü öğesi
 * @param {boolean} isSectionHeader - Menü öğesi bir bölüm başlığı mı?
 */
function closeOtherMenusInSameLevel(currentItem, isSectionHeader) {
    const parent = currentItem.closest('.submenu, .section-submenu, .sidebar-menu');
    if (!parent) return;

    // Aynı ebeveyn içindeki tüm menüleri bul
    const siblings = parent.querySelectorAll(isSectionHeader ?
        '.section-header[aria-expanded="true"]' :
        '.collapsible-menu-item:not(.section-header)[aria-expanded="true"]');

    siblings.forEach(item => {
        // Tıklanan öğe değilse kapat
        if (item !== currentItem) {
            const targetId = item.getAttribute('href');
            if (targetId) {
                const targetElement = document.querySelector(targetId);
                if (targetElement) {
                    // Bootstrap Collapse API kullan veya manuel kapat
                    if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                        const bsCollapse = bootstrap.Collapse.getInstance(targetElement);
                        if (bsCollapse) {
                            bsCollapse.hide();
                        } else {
                            new bootstrap.Collapse(targetElement, {
                                show: false
                            });
                        }
                    } else {
                        // Bootstrap yoksa manuel kapat
                        targetElement.classList.remove('show');
                    }
                    item.setAttribute('aria-expanded', 'false');
                }
            }
        }
    });
}

/**
 * Aktif menüler için işlemler
 */
function initActiveMenuHandling() {
    // Önce tüm açık menüleri kapat
    closeAllMenus();

    // Aktif menüleri bul
    const activeLinks = document.querySelectorAll('.nav-link.active');

    // Her aktif menü için
    activeLinks.forEach(activeLink => {
        // Sadece aktif menünün üst menülerini aç
        expandParentMenus(activeLink);
    });
}

/**
 * Tüm açık menüleri kapatır
 */
function closeAllMenus() {
    const openMenus = document.querySelectorAll('.collapsible-menu-item[aria-expanded="true"]');
    openMenus.forEach(menu => {
        const targetId = menu.getAttribute('href');
        if (targetId) {
            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                // Bootstrap Collapse API kullan
                if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                    const bsCollapse = bootstrap.Collapse.getInstance(targetElement);
                    if (bsCollapse) {
                        bsCollapse.hide();
                    }
                } else {
                    // Bootstrap yoksa manuel kapat
                    targetElement.classList.remove('show');
                }
                menu.setAttribute('aria-expanded', 'false');
            }
        }
    });
}

/**
 * Aktif menünün tüm üst menülerini aç
 * @param {HTMLElement} activeItem - Aktif menü öğesi
 */
function expandParentMenus(activeItem) {
    // Eğer aktif öğe bir menü başlığıysa kendisini değil, sadece üst collapse'ları aç
    if (activeItem.classList.contains('collapsible-menu-item')) {
        // Sadece aktif olduğunda kendi alt menüsünü aç
        const targetId = activeItem.getAttribute('href');
        if (targetId) {
            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                targetElement.classList.add('show');
                activeItem.setAttribute('aria-expanded', 'true');
            }
        }
    }

    // Aktif öğenin en yakın submenu'sünü bul
    let parentCollapse = activeItem.closest('.collapse');

    // Tüm üst menüleri yukarı doğru takip et
    while (parentCollapse) {
        // Collapse'ı göster
        parentCollapse.classList.add('show');

        // Collapse controller'ını bul
        const parentLink = document.querySelector(`[href="#${parentCollapse.id}"]`);
        if (parentLink) {
            // aria-expanded değerini güncelle
            parentLink.setAttribute('aria-expanded', 'true');

            // Bir sonraki üst collapse'ı ara
            parentCollapse = parentLink.closest('.collapse');
        } else {
            break;
        }
    }
}

/**
 * Mobil cihazlar için responsive ayarlar
 */
function initMobileResponsive() {
    // İçerik alanı tıklamaları
    const contentWrapper = document.querySelector('.content-wrapper');
    if (contentWrapper) {
        contentWrapper.addEventListener('click', function() {
            if (window.innerWidth < 768) {
                const sidebar = document.querySelector('.sidebar');
                if (sidebar && sidebar.classList.contains('active')) {
                    sidebar.classList.remove('active');
                    contentWrapper.classList.remove('active');
                }
            }
        });
    }

    // Pencere boyutu değiştiğinde
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            const sidebar = document.querySelector('.sidebar');
            if (sidebar) {
                sidebar.classList.remove('active');
            }
            if (contentWrapper) {
                contentWrapper.classList.remove('active');
            }
        }
    });
}

/**
 * Bootstrap bileşenlerini başlat
 */
function initBootstrapComponents() {
    // Tooltip'leri başlat
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }

    // Popover'ları başlat
    if (typeof bootstrap !== 'undefined' && bootstrap.Popover) {
        const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
        [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));
    }
}

/**
 * Menü öğesini manuel olarak aç
 * @param {string} menuId - Açılacak menünün ID'si
 */
function expandMenu(menuId) {
    const menuElement = document.getElementById(menuId);
    if (!menuElement) return;

    // Collapse elementini göster
    menuElement.classList.add('show');

    // Controller'ı bul ve güncelle
    const controller = document.querySelector(`[href="#${menuId}"]`);
    if (controller) {
        controller.setAttribute('aria-expanded', 'true');
    }

    // Üst menüleri de aç
    expandParentMenus(menuElement);
}

/**
 * Menü öğesini manuel olarak kapat
 * @param {string} menuId - Kapatılacak menünün ID'si
 */
function collapseMenu(menuId) {
    const menuElement = document.getElementById(menuId);
    if (!menuElement) return;

    // Collapse elementini gizle
    menuElement.classList.remove('show');

    // Controller'ı bul ve güncelle
    const controller = document.querySelector(`[href="#${menuId}"]`);
    if (controller) {
        controller.setAttribute('aria-expanded', 'false');
    }
}

/**
 * Tüm bildirim kutularını otomatik kapat
 */
(function() {
    // Tüm kapatılabilir uyarı kutularını bul
    const alerts = document.querySelectorAll('.alert-dismissible');

    // Her uyarıya otomatik kapanma fonksiyonu ekle
    alerts.forEach(alert => {
        // 5 saniye sonra otomatik kapat
        setTimeout(() => {
            // Bootstrap dismiss API'sini kullan
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } else {
                // Bootstrap yoksa manuel kapat
                alert.classList.add('fade');
                setTimeout(() => {
                    alert.remove();
                }, 150);
            }
        }, 5000);
    });
})();