/**
 * Monitors Panel API
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { ImageType } from './image-type.model';


export interface CreateImageCommand { 
    imageName?: string | null;
    slug?: string | null;
    imageType?: ImageType;
    requiredSpawnParameters?: Array<string> | null;
}

