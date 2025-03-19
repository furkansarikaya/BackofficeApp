// Sidebar ve menü işlevselliği
document.addEventListener('DOMContentLoaded', function() {
    // Sidebar toggle butonu işlevselliği
    setupSidebarToggle();

    // Mini sidebar modu
    setupMiniSidebar();

    // Açılır-kapanır menü öğeleri
    initCollapsibleMenus();

    // Mobil cihazlarda sidebar kapatma ve diğer responsive işlemler
    setupMobileResponsive();

    // Aktif menüyü açık tut, diğerlerini kapat
    setupActiveMenu();
});

// Aktif menüyü açık tut, diğerlerini kapat
function setupActiveMenu() {
    // Önce aktif olan menü öğelerini bulalım
    const activeMenuItems = document.querySelectorAll('.nav-link.active');

    // Aktif öğe varsa, onun tüm üst menülerini açalım
    if (activeMenuItems.length > 0) {
        activeMenuItems.forEach(activeItem => {
            // Aktif öğenin tüm üst menülerini açık hale getir
            ensureParentMenusOpen(activeItem);
        });
    }

    // Aktif olmayan ve açık durumda olan menüleri kapat
    closeNonActiveMenus();
}

// Aktif öğenin tüm üst menülerini açık duruma getir
function ensureParentMenusOpen(activeItem) {
    // Önce aktif öğenin en yakın üst menüsünü bulalım
    let parentSubmenu = activeItem.closest('.submenu, .section-submenu');

    while (parentSubmenu) {
        // Bu alt menünün bağlı olduğu menü öğesini bulalım (collapse trigger)
        const parentMenuId = parentSubmenu.id;
        if (parentMenuId) {
            const parentMenuItem = document.querySelector(`[href="#${parentMenuId}"]`);

            if (parentMenuItem) {
                // Üst menüyü açık olarak işaretle
                parentMenuItem.setAttribute('aria-expanded', 'true');

                // Collapse sınıfını da ekleyelim
                parentSubmenu.classList.add('show');

                // Bir sonraki üst menüye geçelim
                parentSubmenu = parentMenuItem.closest('.submenu, .section-submenu');
            } else {
                break;
            }
        } else {
            break;
        }
    }

    // Eğer aktif öğe bir bölüm başlığı altında ise, o bölümü de açık hale getir
    const parentSectionSubmenu = activeItem.closest('.section-submenu');
    if (parentSectionSubmenu) {
        const sectionId = parentSectionSubmenu.id.replace('section-', '');
        if (sectionId) {
            const sectionHeader = document.querySelector(`[href="#section-${sectionId}"]`);
            if (sectionHeader) {
                sectionHeader.setAttribute('aria-expanded', 'true');
                parentSectionSubmenu.classList.add('show');
            }
        }
    }
}

// Aktif olmayan ve açık durumda olan menüleri kapat
function closeNonActiveMenus() {
    // Açık durumdaki tüm collapsible öğeleri bul
    const expandedMenus = document.querySelectorAll('.collapsible-menu-item[aria-expanded="true"]');

    expandedMenus.forEach(item => {
        const targetId = item.getAttribute('href');
        if (!targetId) return;

        const targetElement = document.querySelector(targetId);
        if (!targetElement) return;

        // Eğer bu menü aktif bir menü öğesi içermiyorsa kapat
        const hasActiveChild = targetElement.querySelector('.nav-link.active') !== null;

        if (!hasActiveChild && !item.classList.contains('active')) {
            // Menüyü kapat
            const bsCollapse = bootstrap.Collapse.getInstance(targetElement);
            if (bsCollapse) {
                bsCollapse.hide();
            } else {
                // Eğer bootstrap instance yoksa, manuel olarak kapat
                targetElement.classList.remove('show');
            }
            // aria-expanded durumunu güncelle
            item.setAttribute('aria-expanded', 'false');
        }
    });
}

// Sidebar toggle butonu işlevselliğini ayarla
function setupSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.querySelector('.sidebar')?.classList.toggle('active');
            document.querySelector('.content-wrapper')?.classList.toggle('active');
        });
    }
}

// Mini sidebar modunu ayarla
function setupMiniSidebar() {
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

// Açılır-kapanır menüleri başlat
function initCollapsibleMenus() {
    const isMobile = window.innerWidth < 992;
    const collapsibleItems = document.querySelectorAll('.collapsible-menu-item');

    collapsibleItems.forEach(item => {
        // Menü öğesi tıklandığında
        item.addEventListener('click', function(re) {
            const thisTarget = this.getAttribute('href');
            if (!thisTarget) return;

            // Bölüm başlığı kısmını seçiyoruz
            const isSectionHeader = this.classList.contains('section-header');

            // Bir açılır-kapanır menü açıldığında aynı seviyedeki diğer menüleri kapat
            if (isMobile || this.getAttribute('data-close-others') !== 'false') {
                closeOtherMenus(this, isSectionHeader);
            }
        });
    });
}

// Diğer menüleri kapat
function closeOtherMenus(currentItem, isSectionHeader) {
    const collapsibleItems = document.querySelectorAll('.collapsible-menu-item');

    collapsibleItems.forEach(otherItem => {
        // Aynı bölüm başlığı türünde mi?
        const otherIsSectionHeader = otherItem.classList.contains('section-header');

        // Aynı tipte ve kendisi değilse
        if (isSectionHeader === otherIsSectionHeader && otherItem !== currentItem) {
            const otherTarget = otherItem.getAttribute('href');

            // Alt menüler için sadece aynı bölüm içindekileri kapat
            if (!isSectionHeader) {
                // Aynı bölüm içinde mi?
                const thisParent = currentItem.closest('.section-submenu');
                const otherParent = otherItem.closest('.section-submenu');

                // Aynı bölüm içinde değilse geç
                if (thisParent !== otherParent) {
                    return;
                }
            }

            // Açık menüyü kapat
            if (otherTarget && otherItem.getAttribute('aria-expanded') === 'true') {
                closeMenu(otherItem, otherTarget);
            }
        }
    });
}

// Belirli bir menüyü kapat
function closeMenu(item, target) {
    const collapseElement = document.querySelector(target);
    if (collapseElement) {
        // Bootstrap Collapse API kullanarak kapat
        const bsCollapse = bootstrap.Collapse.getInstance(collapseElement);
        if (bsCollapse) {
            bsCollapse.hide();
        }
        item.setAttribute('aria-expanded', 'false');
    }
}

// Menüyü genişlet
function expandMenu(menuElement, triggerElement) {
    const bsCollapse = new bootstrap.Collapse(menuElement, {
        toggle: false
    });
    bsCollapse.show();
    if (triggerElement) {
        triggerElement.setAttribute('aria-expanded', 'true');
    }
}

// Menüyü daralt
function collapseMenu(menuElement, triggerElement) {
    const bsCollapse = new bootstrap.Collapse(menuElement, {
        toggle: false
    });
    bsCollapse.hide();
    if (triggerElement) {
        triggerElement.setAttribute('aria-expanded', 'false');
    }
}

// Mobil cihazlar için responsive ayarları
function setupMobileResponsive() {
    // İçerik alanında tıklama yapıldığında (mobil) sidebar'ı gizle
    const contentWrapper = document.querySelector('.content-wrapper');
    if (contentWrapper) {
        contentWrapper.addEventListener('click', function() {
            const isMobile = window.innerWidth < 992;
            const sidebar = document.querySelector('.sidebar');

            if (isMobile && sidebar && sidebar.classList.contains('active')) {
                sidebar.classList.remove('active');
                document.querySelector('.content-wrapper')?.classList.remove('active');
            }
        });
    }

    // Pencere boyutu değiştiğinde ayarla
    window.addEventListener('resize', function() {
        if (window.innerWidth > 992) {
            document.querySelector('.sidebar')?.classList.remove('active');
            document.querySelector('.content-wrapper')?.classList.remove('active');
        }
    });
}