window.openRazorpayCheckout = function (options, dotNetHelper) {
    var razorpayOptions = {
        "key": options.key,
        "amount": Math.round(options.amount * 100), // convert to paise
        "currency": options.currency,
        "name": "CareSphere Hospital",
        "description": "Invoice Payment " + options.orderId,
        "order_id": options.orderId,
        "handler": function (response) {
            dotNetHelper.invokeMethodAsync('OnPaymentSuccess', response.razorpay_payment_id, response.razorpay_order_id, response.razorpay_signature);
        },
        "prefill": {
            "name": options.patientName || "",
            "email": options.patientEmail || "",
            "contact": options.patientPhone || ""
        },
        "theme": {
            "color": "#0d6efd" // Primary Bootstrap 5 color
        },
        "modal": {
            "ondismiss": function() {
                dotNetHelper.invokeMethodAsync('OnPaymentCancelled');
            }
        }
    };
    
    var rzp = new Razorpay(razorpayOptions);
    
    rzp.on('payment.failed', function (response) {
        dotNetHelper.invokeMethodAsync('OnPaymentFailed', 
            response.error.code || "FAILED", 
            response.error.description || "Payment failed", 
            response.error.metadata.payment_id || "", 
            response.error.metadata.order_id || "");
    });
    
    rzp.open();
};

window.downloadFile = function (fileName, contentType, content) {
    var file = new Blob([content], { type: contentType });
    var exportUrl = URL.createObjectURL(file);
    var a = document.createElement("a");
    a.href = exportUrl;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(exportUrl);
};
