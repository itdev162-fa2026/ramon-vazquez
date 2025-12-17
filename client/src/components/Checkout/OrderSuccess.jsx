import { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { getOrderBySessionId } from "../../services/api";
import "./OrderSuccess.css";

function OrderSuccess({ clearCart }) { // Destructure clearCart prop
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const sessionId = searchParams.get("session_id");

    if (!sessionId) {
      setError("No session ID found");
      setLoading(false);
      return;
    }

    const fetchOrder = async () => {
      try {
        setLoading(true);
        const orderData = await getOrderBySessionId(sessionId);
        setOrder(orderData);
        
        // Clear cart from both storage and state
        localStorage.removeItem("cart");
        if (clearCart) clearCart(); 
        
        setError(null);
      } catch (err) {
        setError("We are still processing your order details. Please refresh in a moment.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchOrder();
  }, [searchParams, clearCart]);

  if (loading) {
    return (
      <div className="order-success-container">
        <div className="loading">Loading order details...</div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="order-success-container">
        <div className="error-state">
          <h2>⚠️ Processing Order</h2>
          <p>{error || "Order not found"}</p>
          <button onClick={() => window.location.reload()} className="home-button" style={{marginBottom: '10px'}}>
            Retry
          </button>
          <button onClick={() => navigate("/")} className="home-button">
            Return to Home
          </button>
        </div>
      </div>
    );
  }

  const getStatusBadge = (status) => {
    switch (status) {
      case 1: 
        return <span className="status-badge status-completed">Completed</span>;
      case 0: 
        return <span className="status-badge status-pending">Pending</span>;
      case 2: 
        return <span className="status-badge status-failed">Failed</span>;
      default:
        return <span className="status-badge">Unknown</span>;
    }
  };

  return (
    <div className="order-success-container">
      <div className="success-content">
        <div className="success-header">
          <div className="success-icon">✓</div>
          <h1>Payment Successful!</h1>
          <p className="success-message">
            Thank you for your order. A confirmation email has been sent to{" "}
            <strong>{order.customerEmail}</strong>
          </p>
        </div>

        <div className="order-details">
          <h2>Order Details</h2>
          <div className="order-info-grid">
            <div className="info-item">
              <span className="info-label">Order ID:</span>
              <span className="info-value">#{order.id}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Status:</span>
              <span className="info-value">{getStatusBadge(order.status)}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Order Date:</span>
              <span className="info-value">
                {new Date(order.createdDate).toLocaleDateString()}
              </span>
            </div>
            <div className="info-item">
              <span className="info-label">Total Amount:</span>
              <span className="info-value total">
                ${order.totalAmount.toFixed(2)}
              </span>
            </div>
          </div>
        </div>

        <div className="order-items">
          <h2>Order Items</h2>
          <div className="items-list">
            {order.orderItems.map((item) => (
              <div key={item.id} className="order-item">
                <div className="item-info">
                  <h4>{item.productName}</h4>
                  <p>Quantity: {item.quantity}</p>
                </div>
                <div className="item-pricing">
                  <span className="item-price">
                    ${item.priceAtPurchase.toFixed(2)} each
                  </span>
                  <span className="item-subtotal">
                    ${(item.priceAtPurchase * item.quantity).toFixed(2)}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="payment-info">
          <p className="stripe-notice">
            🔒 Payment securely processed by Stripe
          </p>
          {order.stripePaymentIntentId && (
            <p className="payment-id">
              Payment ID: {order.stripePaymentIntentId}
            </p>
          )}
        </div>

        <button
          onClick={() => navigate("/")}
          className="continue-shopping-button"
        >
          Continue Shopping
        </button>
      </div>
    </div>
  );
}

export default OrderSuccess;