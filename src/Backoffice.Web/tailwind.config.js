/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: 'class',
    content: [
        "./Pages/**/*.cshtml",
        "./Views/**/*.cshtml",
        "./Areas/**/*.cshtml",
        "./wwwroot/js/**/*.js"
    ],
    theme: {
        extend: {
            colors: {
                // Ana marka renkleri
                primary: {
                    50: '#eff8ff',
                    100: '#dbeefe',
                    200: '#c0e3fd',
                    300: '#94d5fc',
                    400: '#5ebef9',
                    500: '#3aa2f2',
                    600: '#2186e3',
                    700: '#1c6dd2',
                    800: '#1d5aaa',
                    900: '#1c4886',
                    950: '#142e55',
                },
                // Reka UI'den ilham alınan koyu tema renkleri
                dark: {
                    background: '#121212',
                    surface: '#1e1e1e',
                    elevated: '#2c2c2c',
                    border: '#383838',
                    text: {
                        primary: '#ffffff',
                        secondary: '#a0a0a0',
                        muted: '#6e6e6e',
                    },
                },
                // Yeşil vurgu renkleri (gördüğüm yeşil navigasyon çubuğundan)
                accent: {
                    50: '#ecfdf5',
                    100: '#c6f6e5',
                    200: '#9aeed1',
                    300: '#6ee2bc',
                    400: '#38cc9f',
                    500: '#12b981', // Reka UI'daki navigasyon çubuğu rengi
                    600: '#089669',
                    700: '#077456',
                    800: '#085d45',
                    900: '#064e3b',
                    950: '#022c22',
                },
                // Kullanıcı arayüzü gri tonları
                gray: {
                    50: '#f9fafb',
                    100: '#f3f4f6',
                    200: '#e5e7eb',
                    300: '#d1d5db',
                    400: '#9ca3af',
                    500: '#6b7280',
                    600: '#4b5563',
                    700: '#374151',
                    800: '#1f2937',
                    900: '#111827',
                    950: '#030712',
                },
            },
            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
            },
            boxShadow: {
                'soft': '0 2px 10px rgba(0, 0, 0, 0.1)',
                'elevated': '0 8px 30px rgba(0, 0, 0, 0.12)',
                'dark': '0 10px 30px -5px rgba(0, 0, 0, 0.3)',
            },
            borderRadius: {
                'xl': '1rem',
                '2xl': '1.5rem',
            },
        },
    },
    plugins: [],
}