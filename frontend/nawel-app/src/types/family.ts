export interface Family {
  id: number;
  name: string;
  userCount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFamilyData {
  name: string;
}

export interface UpdateFamilyData extends CreateFamilyData {
  id: number;
}
