export interface ResponseDto<T> {
  isSuccess: boolean;
  result: T | null;
  message: string;
  errorMessages: string[];
}

export interface User {
  id: string;
  email: string;
  fullName?: string;
  name?: string;
  phoneNumber?: string;
  roles?: string[];
}

export interface Product {
  productId: number;
  name: string;
  price: number;
  description: string;
  categoryName: string;
  imageUrl?: string;
  imageLocalPath?: string;
}

export interface Coupon {
  couponId: number;
  couponCode: string;
  discountAmount: number;
  minAmount: number;
}

export interface CartItem {
  cartDetailsId: number;
  productId: number;
  count: number;
  product?: Product;
}

export interface Cart {
  cartHeader: {
    cartHeaderId: number;
    userId?: string;
    couponCode?: string;
    discount: number;
    cartTotal: number;
  };
  cartDetails: CartItem[];
}
