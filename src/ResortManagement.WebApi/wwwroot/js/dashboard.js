/* 
   LuxeStay Management Platform - Client Side JS Interactivity 
   Strict adherence to projects/15211795932972419156 Specs 
*/

document.addEventListener("DOMContentLoaded", () => {
    console.log("LuxeStay Management Platform Initialized.");

    // 1. Initialize Custom Toast Alert Container
    const alertContainer = document.createElement("div");
    alertContainer.id = "luxestay-toast-container";
    alertContainer.style.position = "fixed";
    alertContainer.style.top = "24px";
    alertContainer.style.right = "24px";
    alertContainer.style.zIndex = "9999";
    alertContainer.style.display = "flex";
    alertContainer.style.flexDirection = "column";
    alertContainer.style.gap = "12px";
    document.body.appendChild(alertContainer);

    // Global Toast Function
    window.showLuxeAlert = (title, message, type = "success") => {
        const toast = document.createElement("div");
        toast.className = `glass-card toast-alert`;
        toast.style.background = "rgba(255, 255, 255, 0.9)";
        toast.style.backdropFilter = "blur(12px)";
        toast.style.border = "1px solid rgba(255, 255, 255, 0.5)";
        toast.style.boxShadow = "0 10px 30px rgba(0, 39, 65, 0.1)";
        toast.style.padding = "16px 24px";
        toast.style.borderRadius = "12px";
        toast.style.minWidth = "300px";
        toast.style.maxWidth = "400px";
        toast.style.display = "flex";
        toast.style.flexDirection = "column";
        toast.style.gap = "4px";
        toast.style.transform = "translateX(120%)";
        toast.style.transition = "transform 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275)";

        let color = "#006a66"; // Success turquoise
        if (type === "warning") color = "#cd9a5f"; // Gold
        if (type === "danger") color = "#ba1a1a"; // Red

        toast.innerHTML = `
            <div style="display: flex; align-items: center; gap: 8px;">
                <span style="width: 8px; height: 8px; border-radius: 50%; background-color: ${color}; display: inline-block;"></span>
                <strong style="font-size: 14px; color: #002741;">${title}</strong>
            </div>
            <p style="font-size: 13px; color: #42474e; margin-left: 16px;">${message}</p>
        `;

        alertContainer.appendChild(toast);
        
        // Force Reflow
        toast.offsetHeight;
        toast.style.transform = "translateX(0)";

        setTimeout(() => {
            toast.style.transform = "translateX(120%)";
            setTimeout(() => {
                toast.remove();
            }, 500);
        }, 4000);
    };

    // 2. Dynamic Night-by-Night Pricing Calculator on client screen
    const checkInInput = document.getElementById("calc-check-in");
    const checkOutInput = document.getElementById("calc-check-out");
    const basePriceInput = document.getElementById("calc-base-price");
    const priceResultDiv = document.getElementById("calc-price-result");

    if (checkInInput && checkOutInput && basePriceInput && priceResultDiv) {
        const calculatePrice = () => {
            const checkInVal = new Date(checkInInput.value);
            const checkOutVal = new Date(checkOutInput.value);
            const basePrice = parseFloat(basePriceInput.value) || 100.0;

            if (isNaN(checkInVal.getTime()) || isNaN(checkOutVal.getTime()) || checkOutVal <= checkInVal) {
                priceResultDiv.innerHTML = `<span style="color: var(--text-disabled);">Select valid dates to calculate pricing...</span>`;
                return;
            }

            const diffTime = Math.abs(checkOutVal - checkInVal);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            
            let total = 0;
            let breakdownHtml = "";

            for (let i = 0; i < diffDays; i++) {
                let currentDay = new Date(checkInVal);
                currentDay.setDate(checkInVal.getDate() + i);

                let dayPrice = basePrice;
                let markupText = "Base Price";

                // Friday & Saturday weekend markup (e.g. +20%)
                const dayOfWeek = currentDay.getDay();
                if (dayOfWeek === 5 || dayOfWeek === 6) {
                    dayPrice *= 1.2;
                    markupText = "Weekend Rate (+20%)";
                }

                total += dayPrice;
                breakdownHtml += `
                    <div style="display: flex; justify-content: space-between; font-size: 13px; padding: 4px 0; border-bottom: 1px dashed rgba(0,0,0,0.05);">
                        <span>${currentDay.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })} (${markupText})</span>
                        <strong style="color: var(--primary);">$${dayPrice.toFixed(2)}</strong>
                    </div>
                `;
            }

            priceResultDiv.innerHTML = `
                <div style="margin-top: 16px; display: flex; flex-direction: column; gap: 8px;">
                    <div style="font-size: 14px; font-weight: 600; color: var(--primary); margin-bottom: 4px;">Stay Summary (${diffDays} Nights):</div>
                    ${breakdownHtml}
                    <div style="display: flex; justify-content: space-between; font-size: 16px; font-weight: 700; margin-top: 8px; padding-top: 8px; border-top: 1px solid rgba(0,0,0,0.1);">
                        <span>Total Stay Amount:</span>
                        <span style="color: var(--secondary);">$${total.toFixed(2)}</span>
                    </div>
                </div>
            `;
        };

        checkInInput.addEventListener("change", calculatePrice);
        checkOutInput.addEventListener("change", calculatePrice);
        basePriceInput.addEventListener("change", calculatePrice);
    }
});
