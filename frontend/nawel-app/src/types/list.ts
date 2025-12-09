export interface GiftList {
  id: number;
  userId: number;
  userName: string;
  userAvatar: string;
  avatarUrl: string;
  giftCount: number;
  takenCount: number;
  isChildren: boolean;
}

export interface UserListInfo {
  userId: number;
  userName: string;
  userAvatar: string;
  isOwner: boolean;
}

export interface FamilyList {
  familyId: number;
  familyName: string;
  lists: GiftList[];
}

export interface AllListsResponse {
  families: FamilyList[];
}
