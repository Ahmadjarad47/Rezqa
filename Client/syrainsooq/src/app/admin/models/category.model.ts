export interface CategoryDto {
    id: string;
    title: string;
    image: string;
    description: string;
    createdBy: string;
    createdAt: Date;
    updatedAt?: Date;
}

export interface CreateCategoryRequest {
    title: string;
    image: File;
    description: string;
    createdBy?: string;
}

export interface UpdateCategoryRequest {
    id: string;
    title: string;
    image?: File;
    description: string;
} 