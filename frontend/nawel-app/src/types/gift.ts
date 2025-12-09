export interface Gift {
  id: number;
  name: string;
  description?: string;
  url?: string;
  imageUrl?: string;
  price?: number;
  year: number;
  isTaken: boolean;
  takenByUserId?: number;
  takenByUserName?: string;
  comment?: string;
  isGroupGift: boolean;
  participantCount: number;
  participantNames: string[];
  listId?: number;
}

export interface CreateGiftData {
  name: string;
  description?: string;
  url?: string;
  imageUrl?: string;
  price?: number;
  year: number;
}

export interface UpdateGiftData extends CreateGiftData {
  id: number;
}

export interface ProductInfo {
  title?: string;
  description?: string;
  image?: string;
  price?: number;
  currency?: string;
}

export interface ReserveGiftData {
  comment?: string;
}
